﻿using DigitalBallotPlatform.Ballot.DTOs;
using DigitalBallotPlatform.DataAccess.Context;
using DigitalBallotPlatform.Domain.Data.Interfaces;
using DigitalBallotPlatform.Shared.Logger;
using DigitalBallotPlatform.Shared.Models;
using LinqToDB.EntityFrameworkCore;

namespace DigitalBallotPlatform.Domain.Data.Repositories
{
    public class BallotMaterialRepo(BallotDbContext context, ILogger logger) : 
        GenericRepository<BallotMaterialDTO, BallotDbContext>(context, logger), IBallotMaterialRepo
    {
        public async Task<bool> ExecuteUpdateAsync(BallotMaterialDTO ballotMaterialDTO)
        {
            try
            {
                BallotMaterialModel? ballotMaterial = await Context.BallotMaterials.FirstOrDefaultAsyncEF(b => b.Id == ballotMaterialDTO.Id);
                if (ballotMaterial == null)
                    return false;

                ballotMaterial = await BallotMaterialDTO.MapBallotMaterialModel(ballotMaterialDTO);

                Context.BallotMaterials.Update(ballotMaterial);
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

        public async Task<BallotMaterialDTO?> GetBallotMaterialByIdAsync(int id)
        {
            try
            {
                BallotMaterialModel? ballotmaterial = await Context.BallotMaterials.FirstOrDefaultAsyncEF(b => b.Id == id);

                if (ballotmaterial == null)
                    return null;

                Logger.LogInformation("[INFO] {1} Message: Entity {0} query for Id: {2} was successfull", nameof(BallotMaterialModel), nameof(GetBallotMaterialByIdAsync), id);

                return await BallotMaterialDTO.MapBallotMaterialDto(ballotmaterial);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[ERROR] {2} Message: {0} InnerException: {1}", ex.Message, ex.InnerException!, nameof(GetBallotMaterialByIdAsync));
                throw new ArgumentException(ex.Message);
            }
        }
    }
}
