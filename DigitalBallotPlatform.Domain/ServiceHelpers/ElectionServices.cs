﻿using DigitalBallotPlatform.Ballot.DTOs;
using DigitalBallotPlatform.DataAccess.Context;
using DigitalBallotPlatform.Domain.CompositeDTOs;
using DigitalBallotPlatform.Domain.ServiceInterfaces;
using DigitalBallotPlatform.Election.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DigitalBallotPlatform.Domain.ServiceHelpers
{
    public class ElectionServices : IElectionService
    {
        private readonly ElectionDbContext electionDbContext;

        public ElectionServices(ElectionDbContext electionDbContext)
        {
            this.electionDbContext = electionDbContext;
        }

        public async Task<ElectionWithBallotComposite> GetElectionWithBallotSpecAsync(int electionId)
        {
            var election = await electionDbContext.ElectionSetups
                .Include(e => e.Watermark)
                .Include(e => e.County)
                .Include(e => e.Parties)
                .FirstOrDefaultAsync(e => e.Id == electionId);

            if (election == null)
            {
                throw new ArgumentNullException($"Election for Id: {electionId} couldn't be found.");
            }

            var ballotSpec = await electionDbContext.BallotSpecs
                .Include(bs => bs.BallotCategories)
                .Include(bs => bs.BallotMaterial)
                .FirstOrDefaultAsync(bs => bs.Id == election.BallotSpecsId);

            if(ballotSpec == null)
            {
                throw new ArgumentNullException($"Ballot Specs for Id: {election.BallotSpecsId} couldn't be found.");
            }

            return new ElectionWithBallotComposite
            {
                Election = new ElectionSetupDTO
                {
                    Id = election.Id,
                    ElectionDate = election.ElectionDate,
                    Description = election.Description,
                    WatermarkId = election.WatermarkId,
                    CountyId = election.CountyId,
                    BallotSpecsId = election.BallotSpecsId,
                    Parties = election.Parties!.Select(p => new PartyDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Acronym = p.Acronym,
                    })
                    .ToList()
                },
                BallotSpec = new BallotSpecDTO
                {
                    Id = ballotSpec.Id,
                    Length = ballotSpec.Length,
                    Width = ballotSpec.Width,
                    Pages = ballotSpec.Pages,
                    StubSize = ballotSpec.StubSize,
                    IsTopStub = ballotSpec.IsTopStub,
                    BallotMaterialId = ballotSpec.MaterialId,
                    BallotCategories = ballotSpec.BallotCategories.Select(bc => new BallotCategoryDTO
                    {
                        Id = bc.Id,
                        Category = bc.Category.ToString(),
                        SubCategory = bc.SubCategory.ToString(),
                        LARotation = bc.LARotation.ToString(),
                        Description = bc.Description,
                        IsTestdeck = bc.IsTestdeck,
                        Enabled = bc.Enabled,
                        BallotSpecId = bc.BallotSpecId
                    }).ToList()
                }
            };
        }
    }
}
