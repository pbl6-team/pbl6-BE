using System.Runtime.CompilerServices;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PBL6.Domain.Data;
using PBL6.Domain.Models;
using  PBL6.Application.Contract.Examples.Dtos;
using  PBL6.Application.Contract.Examples;
using PBL6.Common.Exceptions;

namespace PBL6.Application.Services
{
    public class ExampleService : IExampleService
    {
        private readonly IUnitOfwork _unitOfwork;
        private readonly IMapper _mapper;
        private readonly ILogger<ExampleService> _logger;
        private readonly string _className;

        public ExampleService(
            IUnitOfwork unitOfwork,
            IMapper mapper,
            ILogger<ExampleService> logger
        )
        {
            _unitOfwork = unitOfwork;
            _mapper = mapper;
            _logger = logger;
            _className = GetType().Name;
        }

        static string GetActualAsyncMethodName([CallerMemberName] string name = null) => name;

        public async Task<Guid> AddAsync(CreateUpdateExampleDto createUpdateExampleDto)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var example = _mapper.Map<Example>(createUpdateExampleDto);
                example = await _unitOfwork.Examples.AddAsync(example);
                await _unitOfwork.SaveChangeAsync();
                _logger.LogInformation("[{_className}][{method}] End", _className, method);

                return example.Id;

            }
            catch (Exception e)
            {
                _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

                throw;
            }
        }

        public async Task<ExampleDto> DeleteAsync(Guid id)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var example = await _unitOfwork.Examples.FindAsync(id);
                if (example is not null)
                {
                    await _unitOfwork.Examples.DeleteAsync(example);
                    await _unitOfwork.SaveChangeAsync();
                }
                else
                {
                    throw new NotFoundException<Example>(id.ToString());
                }
                _logger.LogInformation("[{_className}][{method}] End", _className, method);

                return _mapper.Map<ExampleDto>(example);
            }
            catch (Exception e)
            {
                _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

                throw;
            }
        }

        public async Task<IEnumerable<ExampleDto>> GetAllAsync()
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var examples = await _unitOfwork.Examples.Queryable().ToListAsync();
                await Task.CompletedTask;
                _logger.LogInformation("[{_className}][{method}] End", _className, method);

                return _mapper.Map<IEnumerable<ExampleDto>>(examples);
            }
            catch (Exception e)
            {
                _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

                throw;
            }
        }

        public async Task<ExampleDto> GetByIdAsync(Guid id)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var example = await _unitOfwork.Examples.FindAsync(id);
                _logger.LogInformation("[{_className}][{method}] End", _className, method);

                return _mapper.Map<ExampleDto>(example);
            }
            catch (Exception e)
            {
                _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

                throw;
            }
        }

        public async Task<ExampleDto> GetByNameAsync(string name)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var example = await _unitOfwork.Examples.GetExampleByName(name);
                _logger.LogInformation("[{_className}][{method}] End", _className, method);

                return _mapper.Map<ExampleDto>(example);
            }
            catch (Exception e)
            {
                _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

                throw;
            }
        }

        public async Task<ExampleDto> UpdateAsync(Guid id, CreateUpdateExampleDto exampleDto)
        {
            var method = GetActualAsyncMethodName();
            try
            {
                _logger.LogInformation("[{_className}][{method}] Start", _className, method);
                var example = await _unitOfwork.Examples.FindAsync(id);
                if (example is not null)
                {
                    _mapper.Map(exampleDto, example);
                    await _unitOfwork.Examples.UpdateAsync(example);
                    await _unitOfwork.SaveChangeAsync();
                }
                else
                {
                    throw new NotFoundException<Example>(id.ToString());
                }

                _logger.LogInformation("[{_className}][{method}] End", _className, method);

                return _mapper.Map<ExampleDto>(example);
            }
            catch (Exception e)
            {
                _logger.LogInformation("[{_className}][{method}] Error: {message}", _className, method, e.Message);

                throw;
            }
        }
    }
}