# Asset Manager

Asset Manager is an ASP.NET MVC application designed to efficiently track and manage company assets. It provides a user-friendly interface for adding, editing, and viewing assets, along with efficient filtering and search capabilities. The application includes features for assigning assets on loan, tracking previously loaned assets, reporting damaged assets, maintaining up-to-date damage records, and recording disposed assets.

## Features

- Add and manage office assets such as IT equipment and devices.
- Search and filter assets by description, manufacturer, type, asset number, serial number, office location, and more.
- Support for multiple offices and asset categories.
- Asset damage report feature
- Asset disposal records
- Real-time filtering.
- Uses Bootstrap for responsive design and jQuery for client-side functionality.
<br>
<img width="1896" alt="image" src="https://github.com/user-attachments/assets/3c116c54-13fb-475d-bf18-a9f85a66849a">

## Database Structure

The database includes the following tables:

Users: Stores user information.
Offices: Contains office location details.
Assets: Main asset records including details like type, manufacturer, and serial numbers.
CheckedOutAssets: Tracks assets lent to users, with due dates and return records.
AssetDisposals: Logs asset disposal information, including reasons and dates.
AssetDamages: Documents any asset damages, including descriptions and repair statuses

<img width="842" alt="image" src="https://github.com/user-attachments/assets/3b01368f-dea8-4f61-8989-9b382e1abd7d">


## Technologies Used

- **ASP.NET Core MVC**: Backend framework for handling server-side logic.
- **Entity Framework Core**: ORM for managing database access and persistence.
- **SQL Server**: Database for storing asset data.
- **Bootstrap**: CSS framework for building responsive layouts.
- **jQuery**: For client-side functionality such as search and filter.

## Getting Started

### Prerequisites

To run this project, you'll need:

- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/mikasimoncelli/AssetManagment.git
   cd AssetManagment
