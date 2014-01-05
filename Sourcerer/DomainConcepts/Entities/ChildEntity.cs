using System;
using Sourcerer.DomainConcepts.Facts;

namespace Sourcerer.DomainConcepts.Entities
{
    [Serializable]
    public abstract class ChildEntity<TParent> : Entity where TParent : Entity
    {
        private readonly TParent _parent;

        protected ChildEntity(TParent parent)
        {
            _parent = parent;
        }

        public TParent Parent
        {
            get { return _parent; }
        }

        protected internal override void Append(IFact fact)
        {
            Parent.Append(fact);
        }
    }
}