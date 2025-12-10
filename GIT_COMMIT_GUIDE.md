# Git Commit Guide for Unity VR Project

## ✅ Files You SHOULD Commit

### Essential Unity Files
- **Assets/** - All your game assets, scripts, prefabs, scenes, materials, textures
  - **IMPORTANT**: Include all `.meta` files! These are crucial for Unity's asset references
- **ProjectSettings/** - Project configuration files (should be committed)
- **Packages/manifest.json** - Package dependencies (should be committed)
- **Packages/packages-lock.json** - Locked package versions (should be committed)
- **.gitignore** - The ignore file itself

### Documentation
- **TELEPORTATION_SETUP_GUIDE.md**
- **VR_INPUT_SYSTEM_SETUP_SUMMARY.md**
- **view_logs.sh** (if it's a project utility script)

---

## ❌ Files You SHOULD NOT Commit (Already in .gitignore)

### Unity Generated Directories
- **Library/** - Unity's internal cache and compiled assets
- **Temp/** - Temporary build files
- **Logs/** - Unity editor logs
- **UserSettings/** - User-specific editor settings
- **obj/** - Object files from compilation
- **Build/** or **Builds/** - Build output directories
- **.utmp/** - Unity temporary build files
- **MemoryCaptures/** - Memory profiling data
- **Recordings/** - Unity Recorder output

### Build Artifacts
- **stimulation.apk** - Android build file
- **stimulation1.apk** - Android build file
- **Stimulation.app/** - macOS/iOS build directory
- Any **.aab**, **.ipa**, or **.unitypackage** files

### Auto-Generated Project Files
- **Assembly-CSharp.csproj**
- **Assembly-CSharp-Editor.csproj**
- **Unity.XR.Interaction.Toolkit.Samples.*.csproj**
- **XCharts.*.csproj**
- **My project (2).sln**
- **VR_Kinematics_Physics.sln**
- Any **.user**, **.pidb**, **.booproj**, **.svd** files

### Debug & Development Files
- **My project (2)_BurstDebugInformation_DoNotShip/** - Burst compiler debug info
- **XCharts-Daemon/** - XCharts generated files
- **crash*.log** - Crash reports
- **sysinfo.txt** - System information dumps

### IDE & OS Files
- **.vs/** - Visual Studio settings
- **.idea/** - JetBrains IDE settings
- **.DS_Store** - macOS Finder metadata
- All other OS-specific hidden files

---

## 📋 Quick Checklist Before Committing

Before pushing to GitHub, verify:

- [ ] No `.apk` or `.app` files in the commit
- [ ] No `Library/` directory
- [ ] No `Temp/` directory
- [ ] No `Logs/` directory
- [ ] No `UserSettings/` directory
- [ ] No `.csproj` or `.sln` files
- [ ] All `.meta` files in Assets/ are included
- [ ] `ProjectSettings/` is included
- [ ] `Packages/manifest.json` is included
- [ ] `Packages/packages-lock.json` is included

---

## 🔍 How to Check What Will Be Committed

Run these commands before committing:

```bash
# See what files are staged
git status

# See what will be committed (detailed)
git diff --cached --name-only

# Check for common files that shouldn't be committed
git ls-files | grep -E '\.(apk|app|csproj|sln)$'
git ls-files | grep -E '(Library|Temp|Logs|UserSettings)/'
```

---

## 🚨 If You Accidentally Committed These Files

If you've already committed files that shouldn't be there:

1. **Remove from Git (but keep locally):**
   ```bash
   git rm --cached stimulation.apk
   git rm --cached -r Library/
   ```

2. **Add to .gitignore** (already done)

3. **Commit the removal:**
   ```bash
   git commit -m "Remove build artifacts and generated files"
   ```

4. **If already pushed, others will need to:**
   ```bash
   git pull
   git clean -fd  # Remove untracked files
   ```

---

## 📝 Notes

- **`.meta` files are IMPORTANT**: These files maintain Unity's asset GUIDs and references. Always commit them!
- **ProjectSettings should be committed**: These contain project-wide settings that team members need
- **Packages should be committed**: The manifest files ensure everyone uses the same package versions
- **Build files are large**: APK and app bundles can be hundreds of MB - never commit them

