using System;
using MemoRedis.API.Data;
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
        public ActionResult<Memory?> Memorize(Memory memory)
        {
            _repo.CreateMemory(memory);

            return CreatedAtRoute(nameof(RememberById), new { Id = memory.Id }, memory);
        }

        [HttpGet(Name = "RememberAllThings")]
        public ActionResult<Memory?> RememberAllThings()
        {
            return Ok(_repo.GetAllMemories());
        }
    }
}