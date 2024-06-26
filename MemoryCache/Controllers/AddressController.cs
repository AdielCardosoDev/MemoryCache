using MemoryCache.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace MemoryCache.Controllers
{  
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AddressController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;
        private const string COUNTTRIES_KEY = "Countries";
        public AddressController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        [HttpGet]        
        public async Task<IActionResult> GetAddressAsync()
        {
            const string url = "https://viacep.com.br/ws/01001000/json/";

            // Vai ser validado se já existe em cache!
            if(_memoryCache.TryGetValue(COUNTTRIES_KEY, out Address address))
            {
                return Ok(address);
            }

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    address = JsonSerializer.Deserialize<Address>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    // Opções ao salvar em cache!
                    var memoryCacheEntryOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3600), //Tempo expiração!
                        SlidingExpiration = TimeSpan.FromSeconds(1200), //se não for acessado nesse tempo vai ser excluido da memoria!
                    };

                    //Adiciona ao cache em memoria!
                    _memoryCache.Set(COUNTTRIES_KEY, address, memoryCacheEntryOptions);                    

                    return Ok(address);
                }
                else
                {
                    throw new HttpRequestException($"Request to {url} failed with status code {response.StatusCode}.");
                }
            }
        }
    }
}
