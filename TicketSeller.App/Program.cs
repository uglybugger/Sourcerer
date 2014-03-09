using System;
using System.Diagnostics;
using System.Linq;
using Sourcerer;
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
            SourcererConfigurator.Configure().Abracadabra();

            var ticketSeller = new TicketSeller();
            ticketSeller.SellABunchOfTickets();
        }

        public class TicketSeller
        {
            public void SellABunchOfTickets()
            {
                var numCustomers = 1000;

                var eventId = CreateANewEvent();

                var sw = Stopwatch.StartNew();

                Enumerable.Range(0, numCustomers)
                          .Select(SignUpANewCustomer)
                          .Do(customerId => ReserveATicketForCustomer(customerId, eventId))
                          .Done();

                sw.Stop();

                Console.WriteLine("{0} customers reserved tickets in {1} seconds", numCustomers, sw.Elapsed.TotalSeconds);
                Console.WriteLine("{0} transactions per second", numCustomers/sw.Elapsed.TotalSeconds);
                Console.ReadKey();
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
        }
    }
}