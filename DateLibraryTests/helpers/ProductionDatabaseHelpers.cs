﻿using DataLibrary.Context;
using Microsoft.EntityFrameworkCore;

namespace DateLibraryTests.helpers;

public static class ProductionDatabaseHelpers
{
    private static string ReadSqlFile(string fileName) {
        return File.ReadAllText($"../../../helpers/SqlStatements/{fileName}.sql");
    }
    public static void SeedProductionData(SqliteContext context)
    {

        context.Database.ExecuteSqlRaw(ReadSqlFile("muscle"));
        context.Database.ExecuteSqlRaw(ReadSqlFile("exercise"));
        context.Database.ExecuteSqlRaw(ReadSqlFile("training_type"));
        context.Database.ExecuteSqlRaw(ReadSqlFile("exercise_muscle"));
        context.Database.ExecuteSqlRaw(ReadSqlFile("exercise_how_to"));
        // context.Database.ExecuteSqlRaw(insertEquipment);
        context.Database.ExecuteSqlRaw(ReadSqlFile("exercise_type"));
    }

    /// <summary>
    /// creates 4 new users and assigns hashed passwords and salts.
    /// 1. Canaan, `كنعان لازم يتدرب !`,  salt:`wnGHc9SponguR0Givr2Zcvy7UI3Szs0y0lYfAwdatUE=`, hash:`iUXNkjg5RS/0/uEH8f6tasaKKKt8IVsZgTTtyGH3dmQ=`
    /// 2. Dante, `pizza is pizza!`, salt:`UqfeVl+kUvwURJ2Avx5nw2+qTBMKUUdrHE5m48RGFU0=`, hash:`BzMDx05wo6wjTriEEjxDsb9jZTa6QkbeFAr7B46CAwc=`
    /// 3. Alphrad, `sneaky snake`, salt:`ROtBSjEmAd8swaKpy0e7yV6n5IJl+K5i6yF/brfjTD0=`, hash:`wA+0773CBog+MlcEjrNcAmHS6pSd06ceminYzPU4wlI=`
    /// 4. Nero, `ろまである!`, salt:`a5MaAfB/7zpN8dd3+i10RgHGJ455lpoSIOrAXXXQbyY=`, hash:`Kw1vjlzykwUqruCdhzdMOQBuOkn4hljD3JLiOdAogWg=` 
    /// </summary>
    /// <param name="context"></param>
    public static void SeedDummyUsers(SqliteContext context)
    {
        string insertUsers = @"
       insert into user (username, email, height, gender) VALUES
            ('Canaan', 'canaan@test.com', 173, 'M'),
            ('Dante', 'dante@test.com', 200, 'M'),
            ('Alphrad', 'alphrad@test.com', 172, 'F'),
            ('Nero', 'nero@test.com', 156, 'F');      
            ";

        // other wise i need a loop, so noh!
        string insertUserPassword = @"
        insert into user_passwords(user_id, password_hash, password_salt)
        VALUES
            (1, 'iUXNkjg5RS/0/uEH8f6tasaKKKt8IVsZgTTtyGH3dmQ=', 'wnGHc9SponguR0Givr2Zcvy7UI3Szs0y0lYfAwdatUE='),
            (2, 'BzMDx05wo6wjTriEEjxDsb9jZTa6QkbeFAr7B46CAwc=', 'UqfeVl+kUvwURJ2Avx5nw2+qTBMKUUdrHE5m48RGFU0='),
            (3, 'wA+0773CBog+MlcEjrNcAmHS6pSd06ceminYzPU4wlI=', 'ROtBSjEmAd8swaKpy0e7yV6n5IJl+K5i6yF/brfjTD0='),
            (4, 'Kw1vjlzykwUqruCdhzdMOQBuOkn4hljD3JLiOdAogWg=', 'a5MaAfB/7zpN8dd3+i10RgHGJ455lpoSIOrAXXXQbyY=');";

        context.Database.ExecuteSqlRaw(insertUsers);
        context.Database.ExecuteSqlRaw(insertUserPassword);
        context.SaveChanges();
    }
}