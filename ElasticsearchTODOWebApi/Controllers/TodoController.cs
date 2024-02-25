using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System;
using ElasticsearchTODOWebApi.Models;
using Nest;

namespace ElasticsearchTODOWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class TodoController : ControllerBase
    {
        private readonly ILogger<TodoController> _logger;
        private readonly IElasticSearchManager<TodoModel> _elasticSearchManager;
        private readonly string indexName = typeof(TodoModel).Name.ToLower();
        public TodoController(ILogger<TodoController> logger, IElasticSearchManager<TodoModel> elasticSearchManager)
        {
            _logger = logger;
            _elasticSearchManager = elasticSearchManager;
        }

        [HttpPost]
        [Route("InsertDocument")]
        public async Task<IActionResult> InsertDocument([FromBody] TodoModel request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Todo modelindeki eleman eklenecektir.");

            request.CreateTime = DateTime.UtcNow;
            request.UpdateTime = null;
            await _elasticSearchManager.InsertDocumentAsync(indexName, request, cancellationToken);
            return Ok();
        }

        [HttpPost]
        [Route("InsertDocuments")]
        public async Task<IActionResult> InsertDocuments([FromBody] List<TodoModel> request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Todo modelindeki {request?.Count} eleman eklenecektir.");

            foreach (var item in request)
            {
                item.CreateTime = DateTime.UtcNow;
                item.UpdateTime = null;
            }
            await _elasticSearchManager.InsertDocumentsAsync(indexName, request, cancellationToken);
            return Ok();
        }

        [HttpPut]
        [Route("UpdateDocument")]
        public async Task<IActionResult> UpdateDocument([FromBody] TodoModel request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Todo modelindeki eleman güncellenecektir.");
            request.UpdateTime = DateTime.UtcNow;
            await _elasticSearchManager.UpdateDocumentAsync(indexName, request, cancellationToken);
            return Ok();
        }

        [HttpDelete]
        [Route("RemoveDocument/{id:guid}")]
        public async Task<IActionResult> RemoveDocument(Guid id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Todo modelindeki eleman silinecektir.");
            var result = await _elasticSearchManager.RemoveDocumentAsync(indexName, id, cancellationToken);
            return Ok(result);
        }

        [HttpDelete]
        [Route("DeleteIndex")]
        public async Task<IActionResult> DeleteIndex(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Todo modeline ait ({indexName}) index'i silinecektir.");
            var result = await _elasticSearchManager.DeleteIndexAsync(indexName, cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetDocument/{id:guid}")]
        public async Task<IActionResult> GetDocument(Guid id, CancellationToken cancellationToken)
        {
            var result = await _elasticSearchManager.GetDocumentAsync(indexName, id, cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetAllDocuments")]
        public async Task<IActionResult> GetAllDocuments(CancellationToken cancellationToken)
        {
            var result = await _elasticSearchManager.GetAllDocumentsAsync(indexName, cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetQuery")]
        public async Task<IActionResult> GetQuery(string searchText, int skip, int take, CancellationToken cancellationToken)
        {
            var searchQuery = new SearchDescriptor<TodoModel>()
            .Query(sq =>
                sq.MultiMatch(mm => mm
                    .Query(searchText))
            );
            var result = await _elasticSearchManager.GetQueryAsync(indexName, searchQuery, skip, take, cancellationToken);
            return Ok(result);
        }
    }
}
