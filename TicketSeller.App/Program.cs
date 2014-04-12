using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Sourcerer;
using Sourcerer.FactStore.SqlServer;
using Sourcerer.Infrastructure;
using Sourcerer.Persistence.Disk;
using Sourcerer.Persistence.Memory;
using ThirdDrawer.Extensions.CollectionExtensionMethods;
using ThirdDrawer.Extensions.StringExtensionMethods;
using TicketSeller.App.Domain.CustomerAggregate;
using TicketSeller.App.Domain.EventAggregate;

namespace TicketSeller.App
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var factStoreDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TicketSeller");
            var typesProvider = new AssemblyScanningTypesProvider(new[] {typeof (Program).Assembly});
            var diskFactStore = new DiskFactStore(factStoreDirectoryPath, typesProvider);

            var sqlFactStore = SqlServerFactStore.Create(@"Server=.\SQLEXPRESS;Database=TicketSeller;Trusted_Connection=True;", typesProvider);

            SourcererConfigurator.Configure()
                                 //.With(c => c.FactStore = new MemoryFactStore())
                                 //.With(c => c.FactStore = diskFactStore)
                                 .With(c => c.FactStore = sqlFactStore)
                                 .Abracadabra();

            var ticketSeller = new TicketSeller();
            ticketSeller.SellABunchOfTickets();

            Directory.Delete(factStoreDirectoryPath);
        }

        public class TicketSeller
        {
            public void SellABunchOfTickets()
            {
                var numIterations = 1*1000;

                var eventId = CreateANewEvent();

                var sw = Stopwatch.StartNew();

                Enumerable.Range(0, numIterations)
                          .Do(i =>
                              {
                                  var customerId = SignUpANewCustomer(i);
                                  ReserveATicketForCustomer(customerId, eventId);
                              })
                          .Done();

                sw.Stop();

                Console.WriteLine("{0} customers signed up and reserved tickets in {1} seconds", numIterations, sw.Elapsed.TotalSeconds);
                Console.WriteLine("{0} ticket-reservation transactions per second", numIterations/sw.Elapsed.TotalSeconds);

                var sw2 = Stopwatch.StartNew();

                Enumerable.Range(0, numIterations)
                          .Do(i => { var ticketsAvailable = QueryTicketsAvailableFor(eventId); })
                          .Done();

                sw2.Stop();
                Console.WriteLine("{0} capacity-querying transactions per second", numIterations/sw2.Elapsed.TotalSeconds);
            }

            private Guid CreateANewEvent()
            {
                using (var unitOfWork = SourcererFactory.CreateUnitOfWork())
                {
                    var eventRepository = SourcererFactory.CreateRepository<Event>(unitOfWork);
                    var @event = Event.Create("Big Event", 1000*1000);
                    eventRepository.Add(@event);
                    unitOfWork.Complete();

                    return @event.Id;
                }
            }

            private Guid SignUpANewCustomer(int customerNumber)
            {
                using (var unitOfWork = SourcererFactory.CreateUnitOfWork())
                {
                    var customerRepository = SourcererFactory.CreateRepository<Customer>(unitOfWork);

                    var customerName = "Customer_{0:00}".FormatWith(customerNumber);
                    var customer = Customer.Create(customerName);
                    customerRepository.Add(customer);

                    unitOfWork.Complete();

                    //Console.WriteLine("Signed up {0}".FormatWith(customerName));
                    return customer.Id;
                }
            }

            private void ReserveATicketForCustomer(Guid customerId, Guid eventId)
            {
                using (var unitOfWork = SourcererFactory.CreateUnitOfWork())
                {
                    var customerRepository = SourcererFactory.CreateRepository<Customer>(unitOfWork);
                    var eventRepository = SourcererFactory.CreateRepository<Event>(unitOfWork);

                    var customer = customerRepository.GetById(customerId);
                    var @event = eventRepository.GetById(eventId);

                    var numTickets = 1;
                    if (!customer.TryReserveTicketsFor(@event, numTickets)) throw new Exception("Failed to reserve tickets.");

                    //Console.WriteLine("Reserved {0} tickets for {1} to {2}".FormatWith(numTickets, customer.Name, @event.Name));
                    unitOfWork.Complete();
                }
            }

            private int QueryTicketsAvailableFor(Guid eventId)
            {
                using (var unitOfWork = SourcererFactory.CreateUnitOfWork())
                {
                    var eventRepository = SourcererFactory.CreateRepository<Event>(unitOfWork);
                    var @event = eventRepository.GetById(eventId);

                    var remainingTicketCount = @event.GetRemainingTicketCount();
                    unitOfWork.Complete();

                    return remainingTicketCount;
                }
            }
        }
    }
}