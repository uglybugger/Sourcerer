using System;

namespace TicketSeller.App.Domain.EventAggregate
{
    [Serializable]
    internal class TicketSale
    {
        private readonly Guid _customerId;
        private readonly int _numTickets;

        public TicketSale(Guid customerId, int numTickets)
        {
            _customerId = customerId;
            _numTickets = numTickets;
        }

        public Guid CustomerId
        {
            get { return _customerId; }
        }

        public int NumTickets
        {
            get { return _numTickets; }
        }
    }
}