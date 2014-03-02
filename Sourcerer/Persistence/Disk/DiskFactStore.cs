using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sourcerer.DomainConcepts.Entities;
using Sourcerer.DomainConcepts.Facts;
using Sourcerer.Infrastructure;
using ThirdDrawer.Extensions.CollectionExtensionMethods;
using ThirdDrawer.Extensions.StringExtensionMethods;

namespace Sourcerer.Persistence.Disk
{
    public class DiskFactStore : IFactStore
    {
        private readonly string _factStoreDirectoryPath;
        private readonly ICustomSerializer _serializer;
        private readonly ITypesProvider _typesProvider;
        private readonly DirectoryInfo _factDirectoryBase;

        private const string _filenameSuffix = "fact.xml";

        public DiskFactStore(string factStoreDirectoryPath, ICustomSerializer serializer, ITypesProvider typesProvider)
        {
            _factStoreDirectoryPath = factStoreDirectoryPath;
            _serializer = serializer;
            _typesProvider = typesProvider;
            _factDirectoryBase = CreateFactDirectoryBase();
        }

        public void AppendAtomically(IFact[] facts)
        {
            if (facts.None()) return;

            var factsAndFilenames = facts
                .Select(f => new Tuple<IFact, string>(f, ConstructFullyQualifiedFileName(f)))
                .ToArray();

            try
            {
                factsAndFilenames
                    .Do(kvp => WriteFactToDisk(kvp.Item2, kvp.Item1))
                    .Done();
            }
            catch (Exception)
            {
                factsAndFilenames
                    .Do(kvp =>
                        {
                            try
                            {
                                File.Delete(kvp.Item2);
                            }
                                // ReSharper disable EmptyGeneralCatchClause
                            catch (Exception)
                                // ReSharper restore EmptyGeneralCatchClause
                            {
                            }
                        })
                    .Done();
                throw;
            }
        }

        public IEnumerable<FactAbout<T>> GetStream<T>(Guid id) where T : IAggregateRoot
        {
            return GetStream(id, typeof (T)).Cast<FactAbout<T>>();
        }

        private IEnumerable<IFact> GetStream(Guid id, Type aggregateType)
        {
            return LoadFactsFrom(GetFactDirectoryFor(aggregateType.Name, id)) //FIXME hack - use StreamName property
                .OrderBy(f => f.UnitOfWorkProperties.FactTimestamp)
                .ThenBy(f => f.UnitOfWorkProperties.UnitOfWorkId)
                .ThenBy(f => f.UnitOfWorkProperties.SequenceNumber)
                ;
        }

        public IEnumerable<Guid> GetAllStreamIds<T>() where T : IAggregateRoot
        {
            return GetAllStreamIds(typeof (T));
        }

        private IEnumerable<Guid> GetAllStreamIds(Type aggregateType)
        {
            var baseDirectory = GetFactDirectoryFor(aggregateType.Name);
            foreach (var streamDirectory in baseDirectory.GetDirectories())
            {
                Guid result;
                if (Guid.TryParse(streamDirectory.Name, out result)) yield return result;
            }
        }

        public IEnumerable<IGrouping<Guid, IFact>> GetAllFactsGroupedByUnitOfWork()
        {
            var allFacts = from t in _typesProvider.AggregateTypes
                           from streamId in GetAllStreamIds(t)
                           from fact in GetStream(streamId, t)
                           select fact;

            return allFacts.GroupBy(f => f.UnitOfWorkProperties.UnitOfWorkId);
        }

        public void ImportFrom(IEnumerable<IFact> facts)
        {
            AppendAtomically(facts.ToArray());  //FIXME this will run out of memory once we have lots of facts.
        }

        private DirectoryInfo CreateFactDirectoryBase()
        {
            var baseDirectory = new DirectoryInfo(_factStoreDirectoryPath);
            if (!baseDirectory.Exists) baseDirectory.Create();
            return baseDirectory;
        }

        private void WriteFactToDisk(string fullyQualifiedFilename, IFact fact)
        {
            using (var stream = File.OpenWrite(fullyQualifiedFilename))
            {
                _serializer.Serialize(stream, fact);
            }
        }

        private IFact ReadFactFromDisk(FileInfo fileInfo)
        {
            using (var stream = fileInfo.OpenRead())
            {
                return _serializer.Deserialize<IFact>(stream);
            }
        }

        private IEnumerable<IFact> LoadFactsFrom(DirectoryInfo directory)
        {
            return directory
                .GetFiles("*" + _filenameSuffix)
                .AsParallel()
                .Select(ReadFactFromDisk)
                ;
        }

        private string ConstructFullyQualifiedFileName(IFact fact)
        {
            var directory = GetFactDirectoryFor(fact.StreamName, fact.AggregateRootId);
            var filename = ConstructFilenameFor(fact);
            var fullyQualifiedFileName = directory.FullName + "\\" + filename;
            return fullyQualifiedFileName;
        }

        private static string ConstructFilenameFor(IFact fact)
        {
            var filename = "{0}.{1}.{2}.{3}{4}".FormatWith(fact.UnitOfWorkProperties.FactTimestamp.Ticks,
                                                           fact.UnitOfWorkProperties.UnitOfWorkId,
                                                           fact.UnitOfWorkProperties.SequenceNumber,
                                                           fact.GetType().Name,
                                                           _filenameSuffix);
            return filename;
        }

        private DirectoryInfo GetFactDirectoryFor(string streamName)
        {
            var path = Path.Combine(_factDirectoryBase.FullName, streamName);
            var directory = new DirectoryInfo(path);
            if (!directory.Exists) directory.Create();
            return directory;
        }

        private DirectoryInfo GetFactDirectoryFor(string streamName, Guid id)
        {
            var path = Path.Combine(GetFactDirectoryFor(streamName).FullName, id.ToString());
            var directory = new DirectoryInfo(path);
            if (!directory.Exists) directory.Create();
            return directory;
        }
    }
}