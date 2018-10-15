using CrossSolar.Controllers;
using CrossSolar.Domain;
using CrossSolar.Models;
using CrossSolar.Repository;
using Microsoft.AspNetCore.Mvc;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CrossSolar.Tests.Controller
{
    public class AnalyticsControllerTests
    {
        private readonly AnalyticsController _analyticsController;

        private readonly Mock<IAnalyticsRepository> _analyticsRepositoryMock = new Mock<IAnalyticsRepository>();
        private readonly Mock<IPanelRepository> _panelRepositoryMock = new Mock<IPanelRepository>();

        public AnalyticsControllerTests()
        {
            _analyticsController = new AnalyticsController(_analyticsRepositoryMock.Object, _panelRepositoryMock.Object);
        }


        [Fact]
        public async Task Get_FetchAllAnalyticsAsync()
        {

            var analytics = await _analyticsController.GetAnalyticsAsync();

            Assert.NotNull(analytics);

            var foundResult = analytics as OkObjectResult;
            Assert.NotNull(foundResult);
            Assert.Equal(200, foundResult.StatusCode);
        }
        
        
        [Fact]
        public async Task Get_GetAnalystics()
        {
            string panelId = "AAAA1111BBBB2222";

            var mockPanels = new List<Panel>()
            {
                new Panel
                {
                    Brand = "Harmony",
                    Latitude = 12.345678,
                    Longitude = 98.765543,
                    Serial = panelId
                }
            }.AsQueryable().BuildMock();

            var mockOneHourElectricities = new List<OneHourElectricity>() {
            new OneHourElectricity()
            {
                DateTime = new DateTime(2018, 7, 7),
                Id = 1,
                KiloWatt = 100,
                PanelId = panelId
            }
                }.AsQueryable().BuildMock();

            _panelRepositoryMock.Setup(m => m.Query()).Returns(mockPanels.Object);
            _analyticsRepositoryMock.Setup(m => m.Query()).Returns(mockOneHourElectricities.Object);

            // Act
            var result = await _analyticsController.Get(panelId);

            // Assert
            Assert.NotNull(result);

            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task Get_DayResultsAnalystics()
        {
            string panelId = "AAAA1111BBBB2222";

            var mockPanels = new List<Panel>()
            {
                new Panel
                {
                    Brand = "Areva",
                    Latitude = 12.345678,
                    Longitude = 98.765543,
                    Serial = panelId
                }
            }.AsQueryable().BuildMock();

            _panelRepositoryMock.Setup(m => m.Query()).Returns(mockPanels.Object);

            var oneDayElect = new List<OneHourElectricity>() {
                new OneHourElectricity()
                {
                    Id = 1,
                    PanelId = panelId,
                    DateTime = new DateTime(2018, 10, 5),
                    KiloWatt = 300
                },
                new OneHourElectricity()
                {
                    Id = 2,
                    PanelId = panelId,
                    DateTime = new DateTime(2018, 10, 5),
                    KiloWatt = 500
                },
                new OneHourElectricity()
                {
                    Id = 3,
                    PanelId = panelId,
                    DateTime = new DateTime(2018, 10, 13),
                    KiloWatt = 600
                }
            }.AsQueryable().BuildMock();

            _analyticsRepositoryMock.Setup(m => m.Query()).Returns(oneDayElect.Object);
            
            var result = await _analyticsController.DayResults(panelId);

            // Assert
            Assert.NotNull(result);

            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task Post_Analytic()
        {
            var panelId = "21";
            var analytic = new OneHourElectricityModel
            {
                Id = 1,
                KiloWatt = 1,
                DateTime = DateTime.UtcNow
            };

            // Act
            var result = await _analyticsController.Post(panelId, analytic);

            // Assert
            Assert.NotNull(result);

            var createdResult = result as CreatedResult;
            Assert.NotNull(createdResult);
            Assert.Equal(201, createdResult.StatusCode);
        }

    }
}
