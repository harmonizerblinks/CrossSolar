using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrossSolar.Domain;
using CrossSolar.Models;
using CrossSolar.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrossSolar.Controllers
{
    [Route("panel")]
    public class AnalyticsController : Controller
    {
        private readonly IAnalyticsRepository _analyticsRepository;

        private readonly IPanelRepository _panelRepository;

        public AnalyticsController(IAnalyticsRepository analyticsRepository, IPanelRepository panelRepository)
        {
            _analyticsRepository = analyticsRepository;
            _panelRepository = panelRepository;
        }


        // GET api/analytics
        [HttpGet("Analytics")]
        public async Task<IActionResult> GetAnalyticsAsync()
        {
            var analytics = _analyticsRepository.Query();

            return Ok(analytics);
        }

        // GET panel/XXXX1111YYYY2222/analytics
        [HttpGet("{panelId}/[controller]")]
        public async Task<IActionResult> Get([FromRoute] string panelId)
        {
            var panel = await _panelRepository.Query()
                .FirstOrDefaultAsync(x => x.Serial.Equals(panelId.ToLower(), StringComparison.CurrentCultureIgnoreCase));

            if (panel == null) return NotFound();

            var analytics = await _analyticsRepository.Query()
                .Where(x => x.PanelId.Equals(panelId, StringComparison.CurrentCultureIgnoreCase)).ToListAsync();

            var result = new OneHourElectricityListModel
            {
                OneHourElectricitys = analytics.Select(c => new OneHourElectricityModel
                {
                    Id = c.Id,
                    KiloWatt = c.KiloWatt,
                    DateTime = c.DateTime
                })
            };

            return Ok(result);
        }

        // GET panel/XXXX1111YYYY2222/analytics/day
        [HttpGet("{panelId}/[controller]/day")]
        public async Task<IActionResult> DayResults([FromRoute] string panelId)
        {
            //var result = new List<OneDayElectricityModel>();

            var todayAnalytics = await _analyticsRepository.Query()
                .Where(t => t.PanelId.Equals(panelId, StringComparison.CurrentCultureIgnoreCase)).Select(
                    t => new
                    {
                        DateTime = new DateTime(t.DateTime.Year, t.DateTime.Month, t.DateTime.Day, 0, 0, 0),
                        KiloWatt = t.KiloWatt
                    }).GroupBy(d => d.DateTime).Select(
                        g => new OneDayElectricityModel()
                        {
                            DateTime = g.Key,
                            Sum = g.Sum(s => s.KiloWatt),
                            Average = g.Average(a => a.KiloWatt),
                            Minimum = g.Min(m => m.KiloWatt),
                            Maximum = g.Max(m => m.KiloWatt)
                        }
                    ).OrderBy(o => o.DateTime).ToListAsync();

            return Ok(todayAnalytics);
        }

        // POST panel/XXXX1111YYYY2222/analytics
        [HttpPost("{panelId}/[controller]")]
        public async Task<IActionResult> Post([FromRoute] string panelId, [FromBody] OneHourElectricityModel value)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var oneHourElectricityContent = new OneHourElectricity
            {
                PanelId = panelId,
                KiloWatt = value.KiloWatt,
                DateTime = DateTime.UtcNow
            };

            await _analyticsRepository.InsertAsync(oneHourElectricityContent);

            var result = new OneHourElectricityModel
            {
                Id = oneHourElectricityContent.Id,
                KiloWatt = oneHourElectricityContent.KiloWatt,
                DateTime = oneHourElectricityContent.DateTime
            };

            return Created($"panel/{panelId}/analytics/{result.Id}", result);
        }

    }
}