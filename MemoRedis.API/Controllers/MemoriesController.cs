using System;
using MemoRedis.API.Data;
using MemoRedis.API.Dtos;
using MemoRedis.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace MemoRedis.API.Controllers
{
    public class MemoriesController : BaseApiController
    {
        private readonly IMemoryRepository _repo;

        public MemoriesController(IMemoryRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("{id}", Name = "RememberById")]
        public ActionResult<Memory?> RememberById(string id)
        {
            Memory? dbMemory = _repo.GetMemoryById(id);

            if (dbMemory is null)
            {
                return NotFound();
            }

            return Ok(dbMemory);
        }

        [HttpPost(Name = "Memorize")]
        public ActionResult<Memory?> Memorize(CreateMemoryDto memory)
        {
            Memory memoryToAdd = new(Memory.CreateId(Guid.NewGuid()), memory.Description, memory.Date);
            _repo.CreateMemory(memoryToAdd);

            return CreatedAtRoute(nameof(RememberById), new { Id = memoryToAdd.Id }, memoryToAdd);
        }

        [HttpGet(Name = "RememberAllThings")]
        public ActionResult<Memory?> RememberAllThings()
        {
            return Ok(_repo.GetAllMemories());
        }
    }
}