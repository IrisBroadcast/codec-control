using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodecControl.Client;
using CodecControl.Data.Database;
using CodecControl.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CodecControl.Web.CCM
{
    public class CcmDbRepository : ICcmRepository
    {
        private readonly IServiceScope _serviceScope;

        public CcmDbRepository(IServiceProvider serviceProvider)
        {
            _serviceScope = serviceProvider.CreateScope();
        }

        public async Task<List<CodecInformation>> GetCodecInformationList()
        {
            DateTime maxAge = DateTime.UtcNow.AddSeconds(120);

            using (var db = (CcmDbContext)_serviceScope.ServiceProvider.GetService(typeof(CcmDbContext)))
            {
                List<RegisteredSipEntity> rsList = await db.RegisteredSips
                    .Include(rs => rs.UserAgent)
                    .Where(r => r.Updated >= maxAge)
                    .Where(rs => !string.IsNullOrEmpty(rs.UserAgent.Api))
                    .OrderByDescending(rs => rs.Updated)
                    .ToListAsync();

                // Only include latest registrations per sip address.
                var groupBy = rsList.GroupBy(rs => rs.SIP).ToList();
                List<RegisteredSipEntity> groupedList = groupBy.Select(g => g.First())
                    .OrderBy(rs => rs.SIP)
                    .ToList();

                var list = groupedList.Select(rs => new CodecInformation()
                {
                    SipAddress = rs.SIP,
                    Ip = rs.IP,
                    Api = rs.UserAgent?.Api ?? string.Empty,
                    GpoNames = rs.UserAgent?.GpoNames ?? string.Empty,
                    NrOfInputs = rs.UserAgent?.Inputs ?? 0
                }).ToList();

                return list;
            }
        }
    }
}