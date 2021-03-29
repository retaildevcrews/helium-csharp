// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSE.Helium.Model;
using Microsoft.Azure.Cosmos;

namespace CSE.Helium.DataAccessLayer
{
    /// <summary>
    /// Data Access Layer for CosmosDB Interface
    /// </summary>
    public interface IDAL
    {
        Task<Actor> GetActorAsync(string actorId);
        Task<IEnumerable<Actor>> GetActorsAsync(ActorQueryParameters actorQueryParameters);
        Task<IEnumerable<string>> GetGenresAsync();
        Task<Movie> GetMovieAsync(string movieId);
        Task<IEnumerable<Movie>> GetMoviesAsync(MovieQueryParameters movieQueryParameters);
        Task<List<string>> GetFeaturedMovieListAsync();
        Task Reconnect(Uri cosmosUrl, string cosmosKey, string cosmosDatabase, string cosmosCollection, CosmosClient cosmosClient, bool force = false);
    }
}