

🧱 RepositoryAndUnitOfWork

A clean and extensible .NET Core template implementing the Repository and Unit of Work patterns. It emphasizes abstraction, reflection-based operations, and reusable controller logic to minimize boilerplate and maximize flexibility.

This solution reduces the repetition of work at the beginning of every project.


🚀 Features

- Generic repository with reflection-powered CRUD operations
- Unit of Work for transaction consistency
- Auto register for Repository, UOW and service of any domain entity
- Reusable base controllers with filtering and DTO mapping
- Minimal overrides needed for custom logic
- Clean architecture and separation of concerns

---

🧠 Reflection-Based Repository Operations

The repository layer uses reflection to handle standard operations:

- Insert
- Update
- Delete
- Get

This design allows:
- Automatic handling of most entity operations
- Minimal overrides — only needed for very specific cases
- Reduced boilerplate across the data access layer

---

🧩 Base Controllers

Inside the Core/BaseController folder, you'll find a powerful generic controller:

Use `BaseGetController<T, TDetailsDto, TLiteDto, TListDto>` to rapidly expose Get endpoints with filtering and mapping
  or `BaseController<T, TDetailsDto, TCreateDto, TUpdateDto, TLiteDto, TListDto>` to expose all CRUD endpoints,
where `T` is Your domain entity.

These controllers provides:
- Automatic (type/DTO) mapping profile registering
- Built-in filtering, sorting, and pagination
- Generic endpoints for CRUD operations

You can directly inherit from these controllers in your API layer to expose endpoints for any entity with minimal effort. 

This structure ensures:
- Clean separation between domain and presentation
- Easy extension for custom behaviors
- Consistent API design across modules

---

🛠 Getting Started

1. Clone the repository:
   `git clone https://github.com/aaref-sh/RepositoryAndUnitOfWork.git`

2. Restore dependencies:
   `dotnet restore`

3. Build and run:
   `
   dotnet build
   dotnet run --project MainService.API
   `

4. Add your Entities and go a head for logic, 

---

📝 Notes:
- Make all entities inharits (BaseEntity).
- Make all entity's DTOs inharits its baseDto to enable autoMapping register and follow naming policy (eg: ProductListDto)

---

🤝 Contributing

Feel free to fork and extend the project. Contributions are welcome!
