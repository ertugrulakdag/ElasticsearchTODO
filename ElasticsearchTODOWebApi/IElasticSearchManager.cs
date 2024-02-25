namespace ElasticsearchTODOWebApi
{
    public interface IElasticSearchManager<T> where T : class
    {
        Task IsThereIndexAsync(string indexName);//İndex varmı kontrlü
        Task InsertDocumentAsync(string indexName, T document, CancellationToken cancellationToken);//İndex'e doküman ekler
        Task InsertDocumentsAsync(string indexName, List<T> documents, CancellationToken cancellationToken);//İndex'e N kadar doküman ekler
        Task UpdateDocumentAsync(string indexName, T document, CancellationToken cancellationToken);//İndex'e doküman günceller
        Task<bool> RemoveDocumentAsync(string indexName, Guid id, CancellationToken cancellationToken);//İndex'den bir doküman siler
        Task<bool> DeleteIndexAsync(string indexName, CancellationToken cancellationToken);//İndex siler
        Task<T> GetDocumentAsync(string indexName, Guid id, CancellationToken cancellationToken);//İndex'deki 1 dokümanı getirir
        Task<List<T>> GetAllDocumentsAsync(string indexName, CancellationToken cancellationToken);//İndex'deki bütün dokümanları getirir
        Task<List<T>> GetQueryAsync(string indexName, Nest.SearchDescriptor<T> query, int skip, int take, CancellationToken cancellationToken);//İndex'de belirli kriterlere ait dokümanları getirir
    }
}
