using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlightRadar.Controllers;
using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using Moq.Protected;
using Microsoft.EntityFrameworkCore;
using FlightRadar.Models;
namespace FlightRadar.Controllers.Tests
{
    [TestClass()]
    public class UnitTests
    {

        [TestMethod]
        public void BuildLocalTest_BuildSuccesfull_ReturnsTrue_()
        {
            //arrange
            FlightPlan plan = new FlightPlan();
            Flight flight = new Flight();
            plan.id = 1;
            plan.flight_id = "F1";
            plan.company_name = "AllenAir";
            plan.passengers = 100;
            plan.initial_location_longitude = 0;
            plan.initial_location_latitude = 0;
            plan.initial_location_date_time = "2020-06-03T20:00:00Z";
            Tuple<double, double> coordinates = new Tuple<double, double>
                (plan.initial_location_longitude, plan.initial_location_latitude);
            string time = "2020-06-03T20:00:00Z";
            //act
            flight.BuildLocal(plan, coordinates, time);

            //assert
            if (!BuildOK(plan, flight))
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void GetServersList_ServerInserted_ReturnsTrue()
        {
            // Arrange
            DbContextOptions<DataContext> options = new DbContextOptions<DataContext>();
            DataContext context = new DataContext(options);
            Server server = new Server();
            server.id = 1;
            server.ServerId = "1";
            server.ServerURL = "www.ido.com";
            DbSet<Server> servers = context.servers;

            // Act
            servers.Add(server);

            // Assert
            Assert.IsTrue(CheckServerTest(server, servers));
        }

        private bool CheckServerTest(Server server, DbSet<Server> servers)
        {
            foreach (Server entry in servers)
            {
               if(entry.id == server.id) { return true; }
            }
            return false;
        }

        private bool BuildOK(FlightPlan plan, Flight flight)
        {
            return plan.id == flight.id && plan.flight_id == flight.flight_id
                && plan.company_name == flight.company_name && 
                plan.passengers == flight.passengers;
        }
    }
}