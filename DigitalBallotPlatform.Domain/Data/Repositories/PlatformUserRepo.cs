﻿using DigitalBallotPlatform.DataAccess.Context;
using DigitalBallotPlatform.Domain.Data.Interfaces;
using DigitalBallotPlatform.Platform.DTOs;
using DigitalBallotPlatform.Shared.Logger;
using DigitalBallotPlatform.Shared.Models;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DigitalBallotPlatform.Domain.Data.Repositories
{
    public class PlatformUserRepo(ElectionDbContext context, ILogger logger) : 
        GenericRepository<PlatformUserModel, ElectionDbContext>(context, logger), IPlatformUserRepo
    {
        public async Task<bool> ExecuteUpdateAsync(PlatformUserDTO userDto)
        {
            try
            {
                PlatformUserModel? user = await Context.PlatformUsers.AsNoTracking().FirstOrDefaultAsyncEF(u => u.Id == userDto.Id);

                if (user == null)
                {
                    Logger.LogWarning("[WARN] {0} {1} Entity could not be found in the database.", nameof(ExecuteUpdateAsync), this);
                    return false;
                }

                user = await PlatformUserDTO.MapPlatformUserModel(userDto);

                Context.PlatformUsers.Update(user);
                await SaveAsync();

                Logger.LogInformation("[INFO] {1} Message: Entity {0} has been updated", nameof(PlatformUserModel), nameof(ExecuteUpdateAsync));

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[ERROR] {2} Message: {0} InnerException: {1}", ex.Message, ex.InnerException!, nameof(ExecuteUpdateAsync));
                throw new ArgumentException(ex.Message);
            }
        }

        public async Task<PlatformUserDTO?> GetUserByIdAsync(Guid id)
        {
            try
            {
                PlatformUserModel? user = await Context.PlatformUsers.AsNoTracking().FirstOrDefaultAsyncEF(u => u.Id == id);

                if (user == null)
                {
                    Logger.LogWarning("[WARN] {0} {1} Entity could not be found in the database.", nameof(GetUserByIdAsync), this);
                    return null;
                }

                Logger.LogInformation("[INFO] {1} Message: Entity {0} query for Id: {2} was successfull", nameof(PlatformUserModel), nameof(GetUserByIdAsync), id);

                return await PlatformUserDTO.MapPlatformUserDto(user);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[ERROR] {2} Message: {0} InnerException: {1}", ex.Message, ex.InnerException!, nameof(GetUserByIdAsync));
                throw new ArgumentException(ex.Message);
            }
        }

        public async Task<PlatformUserDTO?> ValidateUsernameAsync(string username)
        {
            try
            {
                PlatformUserModel? user = await Context.PlatformUsers.AsNoTracking().SingleOrDefaultAsync(u => u.Username == username);

                if (user == null)
                {
                    Logger.LogWarning("[WARN] {0} {1} Entity could not be found in the database.", nameof(ValidateUsernameAsync), this);
                    return null;
                }

                Logger.LogInformation("[INFO] {1} Message: Entity {0} query for Id: {2} was successfull", nameof(PlatformUserModel), nameof(ValidateUsernameAsync), user.Id);

                return await PlatformUserDTO.MapPlatformUserDto(user);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[ERROR] {2} Message: {0} InnerException: {1}", ex.Message, ex.InnerException!, nameof(ValidateUsernameAsync));
                throw new ArgumentException(ex.Message);
            }
        }
    }
}
