# Building Gas Counter App with Visual Studio 2022

Complete guide for building and deploying the Gas Counter App using Visual Studio 2022.

---

## Prerequisites

### Required Software

1. **Visual Studio 2022** (version 17.8 or later)
   - Download from: https://visualstudio.microsoft.com/downloads/
   - Community Edition is free and sufficient

2. **Required Workloads** during Visual Studio installation:
   - ✅ **.NET Multi-platform App UI development** (MAUI workload)
   - ✅ **Mobile development with .NET** (optional but recommended)

3. **Android SDK Components** (installed automatically with MAUI workload):
   - Android SDK Platform 35 (Android 14)
   - Android SDK Platform 21 (Android 5.0 - minimum)
   - Android SDK Build-Tools
   - Android Emulator

4. **Java Development Kit (JDK)**
   - JDK 11 or later (usually installed with Visual Studio)

### Verify Installation

1. Open Visual Studio 2022
2. Go to **Tools > Options > Xamarin > Android Settings**
3. Verify paths are set correctly:
   - Java SDK Location
   - Android SDK Location
   - Android NDK Location

---

## Opening the Project

### Method 1: Open Solution File

1. **Launch Visual Studio 2022**

2. **Open the solution**:
   - Click **File > Open > Project/Solution**
   - Navigate to: `C:\Users\lmagh\Documents\socar\gas-counter-app\`
   - Select: **socar.sln**
   - Click **Open**

### Method 2: Clone from Git (if needed)

1. **Clone repository**:
   - Click **Git > Clone Repository**
   - Enter repository URL
   - Choose local path
   - Click **Clone**

2. Visual Studio will automatically open the solution after cloning

---

## Project Configuration

### 1. Check Target Framework

1. In **Solution Explorer**, right-click on **GasCounterApp** project
2. Select **Properties**
3. Go to **Application** tab
4. Verify **Target Frameworks**: `net9.0-android`

### 2. Set Startup Project

1. In **Solution Explorer**, right-click on **GasCounterApp** project
2. Select **Set as Startup Project**
3. The project name should now be bold

### 3. Select Build Configuration

In the toolbar:
- **Configuration**: Select **Release** (or **Debug** for testing)
- **Platform**: Select **Any CPU**

---

## Connecting an Android Device

### Option 1: Physical Android Device (Recommended)

**Step 1: Enable Developer Options on Phone**
1. Go to **Settings > About Phone**
2. Tap **Build Number** 7 times
3. You'll see "You are now a developer!"

**Step 2: Enable USB Debugging**
1. Go to **Settings > Developer Options**
2. Enable **USB Debugging**
3. Enable **Install via USB** (if available)

**Step 3: Connect Device**
1. Connect your Android phone to PC via USB cable
2. On your phone, when prompted:
   - Allow USB debugging
   - Select **File Transfer** or **PTP** mode
3. Check if detected: Device should appear in Visual Studio's device dropdown

**Step 4: Verify Connection**
- In Visual Studio toolbar, click the device dropdown
- You should see your device name (e.g., "Samsung Galaxy S21")
- If not visible, click **Android Device Manager** to troubleshoot

### Option 2: Android Emulator

**Step 1: Create Emulator (First Time Only)**
1. In Visual Studio, click **Tools > Android > Android Device Manager**
2. Click **+ New Device**
3. Select a device definition:
   - Recommended: **Pixel 5 - API 35** (Android 14)
   - Or: **Pixel 5 - API 30** (Android 11)
4. Click **Create**
5. Wait for emulator to download and configure (5-10 minutes)

**Step 2: Start Emulator**
1. In **Android Device Manager**, click **Start** on your emulator
2. Wait for emulator to boot (1-2 minutes)
3. Emulator should appear in device dropdown

**Note**: Emulators are slower than physical devices and don't support real GPS.

---

## Building the Project

### Quick Build (F7 or Ctrl+Shift+B)

1. Click **Build > Build Solution** or press **F7**
2. Check **Output** window for build progress
3. Look for: **Build succeeded**

### Build Output Location

After successful build, APK will be at:
```
GasCounterApp\bin\Release\net9.0-android\com.gascounter.app-Signed.apk
```

Or for Debug builds:
```
GasCounterApp\bin\Debug\net9.0-android\com.gascounter.app-Signed.apk
```

---

## Deploying to Device

### Method 1: Run from Visual Studio (Easiest)

1. **Select your device** from dropdown in toolbar
2. Click the **Play button** (green arrow) or press **F5**
   - This will:
     - Build the project
     - Install APK to device
     - Launch the app
     - Attach debugger
3. Wait for deployment (first time: 30-60 seconds, subsequent: 10-20 seconds)
4. App will launch automatically on your device

### Method 2: Run Without Debugging

1. Select your device
2. Press **Ctrl+F5** or click **Debug > Start Without Debugging**
   - Faster than F5 because debugger doesn't attach
   - Better for testing release builds

### Method 3: Deploy Only (No Launch)

1. Right-click **GasCounterApp** project in Solution Explorer
2. Select **Deploy**
3. App will be installed but not launched

---

## Building for Distribution

### Create Release APK

1. **Set Configuration to Release**:
   - Toolbar: Configuration dropdown > **Release**

2. **Clean Solution**:
   - Click **Build > Clean Solution**
   - This removes old build artifacts

3. **Rebuild Solution**:
   - Click **Build > Rebuild Solution**
   - Wait for build to complete

4. **Locate APK**:
   - Build output shows path
   - Default: `GasCounterApp\bin\Release\net9.0-android\`
   - File: `com.gascounter.app-Signed.apk`

5. **Copy APK** to a safe location for distribution

### Sign APK with Custom Keystore (Production)

For production/Play Store releases:

1. **Generate Keystore** (one-time):
   ```bash
   keytool -genkey -v -keystore myapp.keystore -alias myapp -keyalg RSA -keysize 2048 -validity 10000
   ```

2. **Configure in Visual Studio**:
   - Right-click **GasCounterApp** project > **Properties**
   - Go to **Android > Android Package Signing**
   - Check **Sign the .apk file using the following keystore details**
   - Enter keystore path, alias, and passwords

3. **Build** as usual - APK will be signed with your keystore

---

## Debugging in Visual Studio

### Attach Debugger

When running with **F5**, you can:

1. **Set Breakpoints**:
   - Click in left margin of code editor
   - Red dot appears
   - App will pause when breakpoint is hit

2. **Inspect Variables**:
   - Hover over variables to see values
   - Use **Locals** window: Debug > Windows > Locals
   - Use **Autos** window: Debug > Windows > Autos

3. **View Logs**:
   - Open **Output** window: View > Output
   - Select "Debug" from "Show output from:" dropdown
   - See `Console.WriteLine()` output here

### View Android Logs (Logcat)

1. Open **Android Logs** window:
   - View > Other Windows > Android Logs
   - Or: Tools > Android > Android Logs

2. Filter logs:
   - In search box, type: `gascounter`
   - Or select your package name from dropdown

3. View different log levels:
   - Verbose, Debug, Info, Warning, Error

### Common Debug Issues

**"App not installed" error:**
```
Solution:
1. Go to device Settings > Apps > Gas Counter
2. Uninstall the app
3. Try deploying again
```

**"Deployment failed" error:**
```
Solution:
1. Tools > Options > Xamarin > Android Settings
2. Click "Open Android SDK Manager"
3. Ensure all required SDK components are installed
4. Restart Visual Studio
```

**Debugger won't attach:**
```
Solution:
1. On device: Settings > Developer Options
2. Enable "Wait for debugger"
3. Deploy app from Visual Studio
4. App will wait for debugger to attach
```

---

## Performance Tips

### Fast Deployment

1. **Enable Fast Deployment** (Debug builds):
   - Right-click project > Properties
   - Android > Android Options
   - Check "Use Fast Deployment"
   - Speeds up subsequent deployments

2. **Use Debug Configuration** for development:
   - Faster builds
   - Includes debug symbols
   - Easier to troubleshoot

3. **Use Release Configuration** for testing:
   - Optimized performance
   - Smaller APK size
   - Representative of production

### Hot Reload (XAML Changes)

1. **Enable XAML Hot Reload**:
   - Tools > Options > XAML Hot Reload
   - Check "Enable XAML Hot Reload"

2. **Use Hot Reload**:
   - Make changes to XAML files
   - Press Ctrl+S to save
   - Changes appear in running app without rebuilding!

3. **Limitations**:
   - Works for XAML only
   - C# code changes require rebuild

---

## Building from Command Line (Alternative)

If you prefer command line even from Visual Studio:

1. **Open Developer Command Prompt**:
   - Tools > Command Line > Developer Command Prompt

2. **Navigate to project**:
   ```bash
   cd C:\Users\lmagh\Documents\socar\GasCounterApp
   ```

3. **Build**:
   ```bash
   msbuild GasCounterApp.csproj /t:Build /p:Configuration=Release /p:TargetFramework=net9.0-android
   ```

4. **Or use dotnet CLI**:
   ```bash
   dotnet build -f net9.0-android -c Release
   ```

---

## Troubleshooting

### Build Errors

**Error: "SDK not found"**
```
Solution:
1. Tools > Options > Environment > Preview Features
2. Check "Use previews of the .NET SDK"
3. Restart Visual Studio
4. Or: Install .NET 9 SDK manually
```

**Error: "Java not found"**
```
Solution:
1. Visual Studio Installer > Modify
2. Individual Components tab
3. Check "Java JDK development tools"
4. Install
```

**Error: "Android SDK not configured"**
```
Solution:
1. Tools > Android > Android SDK Manager
2. Install required components:
   - Android SDK Platform 35
   - Android SDK Build-Tools
   - Android Emulator
3. Restart Visual Studio
```

### Deployment Errors

**Device not showing in dropdown:**
```
Solutions:
1. Check USB cable is working
2. Try different USB port
3. Restart ADB:
   - Open cmd as Administrator
   - cd C:\Program Files (x86)\Android\android-sdk\platform-tools
   - adb kill-server
   - adb start-server
4. Restart Visual Studio
```

**"Installation failed: INSTALL_FAILED_UPDATE_INCOMPATIBLE"**
```
Solution:
Uninstall existing app from device:
- Device Settings > Apps > Gas Counter > Uninstall
- Or: adb uninstall com.gascounter.app
```

**"Deployment failed with exit code 1"**
```
Solution:
1. Clean solution: Build > Clean Solution
2. Delete bin and obj folders manually
3. Rebuild: Build > Rebuild Solution
```

### Performance Issues

**Build is very slow:**
```
Solutions:
1. Disable antivirus scanning on project folder
2. Exclude bin/obj folders from Windows Search indexing
3. Close other programs
4. Use SSD for project storage
```

**Emulator is slow:**
```
Solutions:
1. Enable hardware acceleration:
   - Tools > Options > Xamarin > Android Settings
   - Check "Enable hardware acceleration"
2. Allocate more RAM to emulator:
   - Android Device Manager > Edit > Advanced
   - Increase RAM to 2048 MB or more
3. Use physical device instead
```

---

## Keyboard Shortcuts

Essential Visual Studio shortcuts:

| Action | Shortcut |
|--------|----------|
| Build Solution | **F7** or **Ctrl+Shift+B** |
| Run (Debug) | **F5** |
| Run (No Debug) | **Ctrl+F5** |
| Stop Debugging | **Shift+F5** |
| Set/Remove Breakpoint | **F9** |
| Step Over | **F10** |
| Step Into | **F11** |
| Find in Files | **Ctrl+Shift+F** |
| Go to Definition | **F12** |
| Comment/Uncomment | **Ctrl+K, Ctrl+C** / **Ctrl+K, Ctrl+U** |
| Format Document | **Ctrl+K, Ctrl+D** |
| Quick Actions | **Ctrl+.** |

---

## Additional Resources

### Visual Studio Documentation
- MAUI Documentation: https://learn.microsoft.com/en-us/dotnet/maui/
- Android Development: https://learn.microsoft.com/en-us/xamarin/android/

### Troubleshooting
- MAUI GitHub Issues: https://github.com/dotnet/maui/issues
- Stack Overflow: Tag `maui` and `xamarin.android`

### Learning Resources
- Microsoft Learn: https://learn.microsoft.com/en-us/training/browse/?products=dotnet-maui
- YouTube: Search "NET MAUI Android development"

---

## Quick Start Checklist

For your first build:

- [ ] Open **socar.sln** in Visual Studio 2022
- [ ] Verify MAUI workload installed
- [ ] Connect Android device via USB
- [ ] Enable USB Debugging on device
- [ ] Authorize device when prompted
- [ ] Select **Release** configuration
- [ ] Select your device from dropdown
- [ ] Press **F5** to build and run
- [ ] Wait for deployment (first time: ~60 seconds)
- [ ] Grant location permission when app launches
- [ ] Test the app!

---

**Last Updated**: November 12, 2025
**Tested with**: Visual Studio 2022 (17.11), .NET 9.0, Android 14
