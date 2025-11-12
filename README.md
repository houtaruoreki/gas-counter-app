# Gas Counter App

A .NET MAUI Android application for tracking gas meter locations and inspection cycles for field technicians in Georgia.

## Features

- **Offline Map Support**: Uses Mapsui with OpenStreetMap for reliable offline mapping
- **GPS Location Tracking**: Automatically captures GPS coordinates when adding counters
- **Counter Management**: Add, edit, search, and track gas counters with detailed information
- **Inspection Cycles**: Track inspection status and manage inspection cycles
- **Search Functionality**: Quick search by counter ID with visual results
- **Backup & Restore**: Create and restore database backups
- **Dark Theme Support**: Full support for light and dark themes
- **Georgian Language UI**: Complete Georgian localization

## Screenshots

### Dashboard
The dashboard provides an overview of inspection progress with statistics and search functionality.

### Map View
Interactive map showing all gas counter locations with pin markers.

### Counter Details
Detailed view for each counter with:
- GPS coordinates and accuracy
- Counter ID and customer information
- Street address
- Counter condition/state
- Inspection status
- Notes

## Technical Stack

- **Framework**: .NET 9.0 with .NET MAUI
- **Database**: SQLite (sqlite-net-pcl v1.9.172)
- **Mapping**: Mapsui v5.0.0 with OpenStreetMap
- **Rendering**: SkiaSharp v3.119.1
- **Target Platform**: Android (API 21+)

## Architecture

The app follows an MVVM-lite pattern:

```
GasCounterApp/
├── Models/          # Data models (GasCounter)
├── Services/        # Business logic (DatabaseService, BackupService)
├── Views/           # XAML UI pages
│   ├── MapPage
│   ├── AddCounterPage
│   ├── CounterDetailPage
│   └── DashboardPage
└── Resources/       # Styles, colors, fonts, images
```

## Database Schema

**GasCounter Table**:
- `Id` (PrimaryKey, AutoIncrement)
- `CounterId` (Text, Optional)
- `CustomerName` (Text, Optional)
- `StreetName` (Text, Optional)
- `Latitude` (Double)
- `Longitude` (Double)
- `Accuracy` (Double, Optional)
- `State` (Text, Optional) - Counter condition
- `IsChecked` (Boolean) - Inspection status
- `Notes` (Text, Optional)
- `CreatedAt` (DateTime)
- `ModifiedAt` (DateTime)
- `LastCheckedAt` (DateTime, Optional)

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Visual Studio 2022 or Visual Studio Code
- Android SDK (API 21 or higher)
- Android device or emulator

### Building the Project

```bash
# Clone the repository
git clone https://github.com/yourusername/gas-counter-app.git
cd gas-counter-app

# Restore dependencies
dotnet restore

# Build for Android (Release)
dotnet build -f net9.0-android -c Release

# The APK will be at:
# GasCounterApp/bin/Release/net9.0-android/com.gascounter.app-Signed.apk
```

### Installing via ADB

**Prerequisites:**
1. Enable Developer Options on your device (Settings > About Phone > tap "Build Number" 7 times)
2. Enable USB Debugging (Settings > Developer Options)
3. Install ADB tools: https://developer.android.com/tools/releases/platform-tools

**Quick Install:**
```bash
# Check device connection
adb devices

# Install or update app
adb install -r "GasCounterApp/bin/Release/net9.0-android/com.gascounter.app-Signed.apk"

# Launch app
adb shell am start -n com.gascounter.app/crc64e39b72f7d7f6e83e.MainActivity
```

**Build and Install Script:**
```bash
# Create a script for one-command deployment:
dotnet build -f net9.0-android -c Release && \
adb install -r "GasCounterApp/bin/Release/net9.0-android/com.gascounter.app-Signed.apk" && \
adb shell am start -n com.gascounter.app/crc64e39b72f7d7f6e83e.MainActivity
```

**Troubleshooting:**
- "device unauthorized": Check phone for USB debugging authorization prompt
- "INSTALL_FAILED_UPDATE_INCOMPATIBLE": Run `adb uninstall com.gascounter.app` first
- For detailed ADB instructions, see [DEVELOPMENT_GUIDE.md](DEVELOPMENT_GUIDE.md#installing-on-device-via-adb)

### Running the App

1. The app will request location permissions on first launch - grant "While using the app"
2. Wait for GPS lock (may take 30 seconds outdoors)
3. Map will auto-center on your location once GPS accuracy is good
4. Tap ➕ to add your first counter

## Usage

### Adding a Counter

1. Navigate to the Map page
2. Tap "Add Counter" button
3. Allow location permission if prompted
4. Fill in counter details (ID, customer name, street, etc.)
5. Select counter condition from the dropdown
6. Add any notes
7. Tap "Save"

### Searching for Counters

1. Go to Dashboard
2. Use the search bar to enter a Counter ID
3. Results appear instantly as you type
4. Tap on a result to view/edit details

### Managing Inspection Cycles

1. View progress on Dashboard (shows total, checked, remaining)
2. Toggle "Checked" status on counter detail page
3. When all counters are checked, app prompts to start new cycle
4. Use "Start New Cycle" button to reset all counters to unchecked

### Backup and Restore

1. Go to Dashboard > Backup section
2. Tap "Create Backup" to save current database
3. Tap "View Backups" to see available backups
4. Select a backup to restore (current data will be replaced)
5. App keeps last 7 backups automatically

## Known Issues

See [bugs.md](bugs.md) for current issues and planned fixes.

## Development

### Project Structure

- **Models**: Plain C# classes representing data entities
- **Services**: Business logic and data access layer
- **Views**: XAML pages with code-behind for UI logic
- **Resources**: Theming, styles, and assets

### Adding New Features

1. Create model classes in `Models/` if needed
2. Add business logic to appropriate service in `Services/`
3. Create XAML page in `Views/` with corresponding code-behind
4. Update navigation in `AppShell.xaml`

### Code Style

- Use async/await for all I/O operations
- Implement proper error handling with try-catch
- Follow MVVM-lite pattern (no heavy framework)
- Use Georgian language for all UI text

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- OpenStreetMap for map data
- Mapsui for the mapping library
- .NET MAUI team for the framework

## Support

For issues, questions, or suggestions, please open an issue on GitHub.

---

**Made with .NET MAUI for gas meter inspection in Georgia**
