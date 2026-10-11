# نمط Installer EXE لـ GeoLocalCAD

## نتائج التحليل الساكن

تم تحليل `EasyCommandPluginInstaller.exe` دون تشغيله. الملف:

- PE32+ x64 .NET/Mono GUI.
- يحتوي على موردين مضمّنين باسم `EasyCommandPlugin.bundle.zip` و`EASY-COMMAND-PLUGIN.png`.
- يضم Bundle Autodesk ApplicationPlugins مع `PackageContents.xml` ونسخ `net48` و`net8` و`net10`.
- يفحص إغلاق AutoCAD/Civil 3D.
- يفك Bundle إلى `C:\ProgramData\Autodesk\ApplicationPlugins`.
- ينشئ مدخل إزالة في `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall`.
- يدعم `/uninstall` ويحذف Demand Load/Bundle ويمسح مدخل الإزالة.
- يحتفظ بمراجع Autodesk خارج الحزمة المصدرية، ويستخدمها وقت البناء.

بصمة الملف الذي تم تحليله:

```text
SHA-256: 50ea2fc3be1c06b906431a862c52855005772d0f6b34bacbdc82412ca9884ec5
```

## تطبيق النمط على GeoLocalCAD

أُضيف:

- `src/GeoLocalCAD.Installer`: EXE WinForms مستقل.
- `Program.cs`: تثبيت/إزالة، embedded bundle، Registry، فحص AutoCAD.
- `scripts/build-installer-exe.ps1`: بناء DLL ثم Bundle ZIP ثم Single-file EXE.
- `app.manifest`: طلب Administrator.

الناتج المتوقع على Windows:

```text
artifacts\GeoLocalCAD-Installer-v0.6.0.exe
```

يجب ألا تُنسخ Autodesk DLLs إلى الـBundle أو EXE؛ يوفرها AutoCAD/Civil 3D على جهاز المستخدم.

## الاختلافات المشروعة

هذا تنفيذ مستقل بنفس **النمط المعماري** العام وليس نسخًا لكود أو موارد EasyCommand. لا تُستخدم أيقونات أو ملفات أو أسماء أو كود مملوك للبرنامج المفحوص.

## القيد الحالي

لا يمكن إنتاج EXE النهائي داخل Sandbox Linux؛ يلزم Windows Self-hosted Runner مثبت عليه .NET SDK وCivil 3D وملفات Autodesk. بعد ذلك يمكن تشغيل:

```powershell
.\scripts\build-installer-exe.ps1 `
  -AutodeskManagedApiPath "C:\Program Files\Autodesk\AutoCAD 2020" `
  -Civil3DManagedApiPath "C:\Program Files\Autodesk\AutoCAD 2020" `
  -GdalRoot "C:\GeoLocalCAD\GDAL"
```
