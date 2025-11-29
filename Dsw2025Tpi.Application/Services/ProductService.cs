using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Domain.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Application.Dtos;
using System.Collections;
using Dsw2025Tpi.Application.Exceptions;

namespace Dsw2025Tpi.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IRepository _productRepository;
        private readonly IEntityMapper<Product, ProductModel.ProductResponse> _entityMapper;
        public ProductService(IRepository productRepository, IEntityMapper<Product, ProductModel.ProductResponse> entityMapper)
        {
            _productRepository = productRepository;
            _entityMapper = entityMapper;
        }

        public async Task<IEnumerable<Product>> GetAllEnabled()
        {
            var activeProducts = await _productRepository.GetFiltered<Product>(p => p.IsActive == true)
                ?? throw new Exception("No hay productos activos.");
            return activeProducts;
        }

        public async Task<ProductModel.ProductResponse> Add(ProductModel.ProductRequest request) 
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "El request no puede ser nulo.");
            }

            // Validacion de campos requeridos

            if ((await _productRepository.First<Product>(p => p.Sku == request.Sku)) != null)
                throw new EntityAlreadyExistsException($"Ya existe un producto con el SKU '{request.Sku}'.");

            if (request.CurrentUnitPrice <= 0)
            {
                throw new ValidationException("El precio unitario debe ser mayor a cero.");
            }

            if(request.StockQuantity < 0)
            {
                throw new ValidationException("La cantidad de stock no puede ser negativa.");
            }

            var productEntity = request.ToEntity();
            var savedProductEntity = await _productRepository.Add(productEntity); 
            return _entityMapper.ToResponse(savedProductEntity);
        }

        public async Task<ProductModel.ProductResponse> Update(Guid id, ProductModel.ProductRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "El request no puede ser nulo.");
            }

            // Validacion de existencia del producto
            var existingProduct = await _productRepository.GetById<Product>(id) ??
                throw new NotFoundException($"El producto de ID {id} no fue encontrado.");

            // Validacion de campos requeridos
            if (request.CurrentUnitPrice <= 0)
            {
                throw new ValidationException("No se puede actualizar. El precio unitario debe ser mayor a cero.");
            }

            if (request.StockQuantity < 0)
            {
                throw new ValidationException("No se puede actualizar. La cantidad de stock no puede ser negativa.");
            }

            // Actualizacion de campos
            existingProduct.Sku = request.Sku;
            existingProduct.InternalCode = request.InternalCode;
            existingProduct.Name = request.Name;
            existingProduct.Description = request.Description;
            existingProduct.CurrentUnitPrice = request.CurrentUnitPrice;
            existingProduct.StockQuantity = request.StockQuantity;

            // Actualizacion de DB
            var savedProduct = await _productRepository.Update(existingProduct);

            return _entityMapper.ToResponse(savedProduct);
        }
        
        public async Task Disable(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id), "El ID no puede ser nulo.");
            }
            var product = await _productRepository.GetById<Product>(id);
            if (product == null)
            {
                throw new NotFoundException($"El producto de ID {id} no fue encontrado.");
            }

            product.Disable();
            await _productRepository.Update(product);
        }

        public async Task<ProductModel.ProductResponse> GetById(Guid id)
        {
            var product = await _productRepository.GetById<Product>(id) ??
                throw new NotFoundException($"El producto de ID {id} no fue encontrado.");
            return _entityMapper.ToResponse(product);

        }

        public async Task<IEnumerable<ProductModel.ProductResponse>> GetAllProducts()
        {
            var products = await _productRepository.GetAll<Product>()
                ?? throw new Exception("No hay productos registrados.");
            return products.Select(p => _entityMapper.ToResponse(p));
        }
    }
}
