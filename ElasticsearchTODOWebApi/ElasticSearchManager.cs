using ElasticsearchTODOWebApi.Models;
using System.Text.Json;
using Elasticsearch.Net;
using Nest;

namespace ElasticsearchTODOWebApi
{
    public class ElasticSearchManager<T> : IElasticSearchManager<T> where T : class
    {
        private readonly IElasticClient _client;
        private readonly IConfiguration _configuration;
        public ElasticSearchManager(IConfiguration configuration)
        {
            _configuration = configuration;
            _client = CreateInstance();
        }
        public async Task IsThereIndexAsync(string indexName)
        {
            var any = await _client.Indices.ExistsAsync(indexName);
            if (any.Exists)
            {
                return;
            }

            var result = await _client.Indices.CreateAsync(indexName,
                    ss => ss.Index(indexName)
                            .Settings(o => o.NumberOfShards(1).NumberOfReplicas(1))
                            .Map<T>(m => m.AutoMap()));

            if (!result.IsValid)
            {
                throw new Exception($"{result.OriginalException?.Message} | {JsonSerializer.Serialize(result.ServerError?.Error)} | {result.DebugInformation}");
            }
        }

        public async Task InsertDocumentAsync(string indexName, T document, CancellationToken cancellationToken)
        {
            await IsThereIndexAsync(indexName);
            if (document == null)
            {
                throw new Exception(typeof(T).Name + " boş olamaz!");
            }

            if (document.GetType().GetProperty("Id") == null)
            {
                throw new Exception($"'{typeof(T).Name}' isimli modelinizde 'Guid?' Tipinde 'Id' property'si olmalıdır!");
            }

            if (document.GetType().GetProperty("Id")?.GetValue(document, null) == null)
            {
                document.GetType().GetProperty("Id")?.SetValue(document, Guid.NewGuid());
            }

            var result = await _client.CreateAsync(document, d => d.Index(indexName), cancellationToken);
            if (!result.IsValid)
            {
                throw new Exception($"{result.OriginalException?.Message} | {JsonSerializer.Serialize(result.ServerError?.Error)} | {result.DebugInformation}");
            }
        }

        public async Task InsertDocumentsAsync(string indexName, List<T> documents, CancellationToken cancellationToken)
        {
            await IsThereIndexAsync(indexName);
            if (documents?.FirstOrDefault()?.GetType().GetProperty("Id") == null)
            {
                throw new Exception($"'{typeof(T).Name}' isimli modelinizde 'Guid?' Tipinde 'Id' property'si olmalıdır!");
            }

            foreach (var document in documents)
            {
                if (document.GetType().GetProperty("Id")?.GetValue(document, null) == null)
                {
                    document.GetType().GetProperty("Id")?.SetValue(document, Guid.NewGuid());
                }
            }


            var result = await _client.IndexManyAsync(documents, indexName, cancellationToken);
            if (!result.ApiCall.Success)
            {
                throw new Exception($"{result.OriginalException?.Message} | {JsonSerializer.Serialize(result.ServerError?.Error)} | {result.DebugInformation}");
            }
        }

        public async Task UpdateDocumentAsync(string indexName, T document, CancellationToken cancellationToken)
        {
            if (document == null)
            {
                throw new Exception(typeof(T).Name + " boş olamaz!");
            }

            if (document.GetType().GetProperty("Id") == null)
            {
                throw new Exception($"'{typeof(T).Name}' isimli modelinizde 'Guid?' Tipinde 'Id' property'si olmalıdır!");
            }

            var result = await _client.UpdateAsync(new DocumentPath<T>(document), u => u.Doc(document).Index(indexName), cancellationToken);

            if (!result.IsValid)
            {
                throw new Exception($"{result.OriginalException?.Message} | {JsonSerializer.Serialize(result.ServerError?.Error)} | {result.DebugInformation}");
            }
        }
        public async Task<bool> RemoveDocumentAsync(string indexName, Guid id, CancellationToken cancellationToken)
        {
            var result = await _client.DeleteAsync<T>(id, d => d.Index(indexName), cancellationToken);
            if (!result.IsValid)
            {
                throw new Exception($"{result.OriginalException?.Message} | {JsonSerializer.Serialize(result.ServerError?.Error)} | {result.DebugInformation}");
            }
            return result.IsValid;
        }

        public async Task<bool> DeleteIndexAsync(string indexName, CancellationToken cancellationToken)
        {
            var result = await _client.Indices.DeleteAsync(indexName, null, cancellationToken);
            if (!result.IsValid)
            {
                throw new Exception($"{result.OriginalException?.Message} | {JsonSerializer.Serialize(result.ServerError?.Error)} | {result.DebugInformation}");
            }
            return result.IsValid;
        }

        public async Task<List<T>> GetAllDocumentsAsync(string indexName, CancellationToken cancellationToken)
        {
            var response = await _client.SearchAsync<T>(q => q.Index(indexName));
            var list = response.Documents.ToList();
            return list;
        }

        public async Task<T> GetDocumentAsync(string indexName, Guid id, CancellationToken cancellationToken)
        {
            var response = await _client.GetAsync<T>(id, q => q.Index(indexName));
            return response.Source;
        }

        public async Task<List<T>> GetQueryAsync(string indexName, SearchDescriptor<T> query, int skip, int take, CancellationToken cancellationToken)
        {
            if (take == 0)
            {
                take = 10;
            }
            query.Index(indexName)
           .Skip(skip)
           .Take(take);
            var response = await _client.SearchAsync<T>(query);
            var list = response.Documents.ToList();
            var json = _client.RequestResponseSerializer.SerializeToString(query);
            return list;
        }



        private ElasticClient CreateInstance()
        {
            var configurationModel = _configuration.GetSection("ElasticSearchConfiguration").Get<ConfigurationModel>();

            var settings = new ConnectionSettings(new Uri(uriString: $"{configurationModel?.Host}:{configurationModel?.Port}"))
            .BasicAuthentication(configurationModel?.UserName, configurationModel?.Password)
            .RequestTimeout(TimeSpan.FromSeconds(300));
            if (!string.IsNullOrEmpty(configurationModel?.FingerPrint))
            {
                settings.CertificateFingerprint(configurationModel?.FingerPrint);
            }

            return new ElasticClient(settings);
        }


    }


}
