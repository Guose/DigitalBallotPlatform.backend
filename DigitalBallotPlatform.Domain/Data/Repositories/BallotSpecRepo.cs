﻿using DigitalBallotPlatform.Ballot.DTOs;
using DigitalBallotPlatform.DataAccess.Context;
using DigitalBallotPlatform.Domain.Data.Interfaces;
using DigitalBallotPlatform.Shared.Logger;
using DigitalBallotPlatform.Shared.Models;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DigitalBallotPlatform.Domain.Data.Repositories
{
    public class BallotSpecRepo(ElectionDbContext context, ILogger logger) : 
        GenericRepository<BallotSpecModel, ElectionDbContext>(context, logger), IBallotSpecRepo
    {
        public async Task<bool> ExecuteUpdateAsync(BallotSpecDTO ballotSpecDTO)
        {
            try
            {
                BallotSpecModel? ballotSpec = await Context.BallotSpecs.AsNoTracking().FirstOrDefaultAsyncEF(b => b.Id == ballotSpecDTO.Id);
                if (ballotSpec == null)
                {
                    Logger.LogWarning("[WARN] {0} {1} Entity could not be found in the database.", nameof(ExecuteUpdateAsync), this);
                    return false;
                }

                ballotSpec = await BallotSpecDTO.MapBallotSpecModel(ballotSpecDTO);

                Context.BallotSpecs.Update(ballotSpec);
                await SaveAsync();

                Logger.LogInformation("[INFO] {1} Message: Entity {0} has been updated", nameof(BallotMaterialModel), nameof(ExecuteUpdateAsync));

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[ERROR] {2} Message: {0} InnerException: {1}", ex.Message, ex.InnerException!, nameof(ExecuteUpdateAsync));
                throw new ArgumentException(ex.Message);
            }
        }

        public async Task<BallotSpecModel?> GetBallotSpecByIdAsync(int id)
        {
            try
            {
                BallotSpecModel? ballotSpec = await Context.BallotSpecs.AsNoTracking().FirstOrDefaultAsyncEF(b => b.Id == id);

                if (ballotSpec == null)
                {
                    Logger.LogWarning("[WARN] {0} {1} Entity could not be found in the database.", nameof(GetBallotSpecByIdAsync), this);
                    return null;
                }

                Logger.LogInformation("[INFO] {1} Message: Entity {0} query for Id: {2} was successfull", nameof(BallotSpecModel), nameof(GetBallotSpecByIdAsync), id);

                return ballotSpec;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[ERROR] {2} Message: {0} InnerException: {1}", ex.Message, ex.InnerException!, nameof(GetBallotSpecByIdAsync));
                throw new ArgumentException(ex.Message);
            }
        }
    }
}
