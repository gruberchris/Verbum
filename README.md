# Verbum

Verbum is a web application for creating, indexing, and searching markdown-based articles. It uses ASP.NET Core Razor Pages and a custom indexing service to provide efficient search functionality.

## Features

- **Create Articles**: Write and upload markdown articles.
- **Indexing**: Automatically index articles on startup and after creation.
- **Search**: Search for articles using indexed terms.

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A code editor like [Visual Studio 2022](https://visualstudio.microsoft.com/vs/), [JetBrains Rider](https://www.jetbrains.com/rider/), or [Visual Studio Code](https://code.visualstudio.com/)

### Installation

1. Clone the repository:
    ```sh
    git clone https://github.com/yourusername/verbum.git
    cd verbum
    ```

2. Restore the dependencies:
    ```sh
    dotnet restore
    ```

3. Build the project:
    ```sh
    dotnet build
    ```

### Running the Application

1. Run the application:
    ```sh
    dotnet run
    ```

2. Open your browser and navigate to `https://localhost:5120`.

### Usage

- **Create Articles**: Navigate to `/CreateArticle` to create a new article.
- **Search Articles**: Use the search bar on the homepage to find articles.

## Project Structure

- `Program.cs`: Configures and runs the application.
- `IndexingService.cs`: Service for indexing and searching articles.
- `CreateArticle.cshtml.cs`: Page model for creating articles.
- `SearchResults.cshtml.cs`: Page model for displaying search results.
- `Article.cshtml.cs`: Page model for displaying individual articles.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

## License

This project is licensed under the GNU General Public License v3.0. See the [LICENSE](LICENSE) file for details.