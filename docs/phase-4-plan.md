# Phase 4 — Coordinate Inspector (`GEOINFO`) Plan

## الهدف

تنفيذ أداة Coordinate Inspector حقيقية داخل Civil 3D باسم `GEOINFO`. تعرض الإحداثيات الأصلية للكائن أو النقطة المحددة، وتحوّلها إلى CRS الرسم أو CRS يحدده المستخدم باستخدام محرك CRS في Phase 3، مع إظهار المصدر والهدف والوحدات ونتيجة التحويل بوضوح.

Phase 4 لا تبدأ بإضافة وظائف Raster أو DEM. الأولوية هي جعل سلسلة الإحداثيات قابلة للفحص والتدقيق قبل توسيع الاستيراد والتصدير.

## نطاق التنفيذ

### 1. Core contracts

إضافة نماذج مستقلة عن Autodesk:

- `CoordinateObservation`: X/Y/Z، نوع المصدر، وقت القراءة، الوحدة.
- `CrsDisplayInfo`: CRS identifier، authority/code، نوع CRS، units، axis order.
- `CoordinateInspectionResult`: source coordinate، transformed coordinate، source CRS، target CRS، residual/status، warnings.
- `ICoordinateInspector`: عقدة اختبارية لا تعتمد على AutoCAD.
- `CoordinateValidation`: رفض NaN وInfinity، رفض CRS فارغ، والتحقق من أن نتيجة PROJ رقمية.

لا يجوز أن يحتوي Core على `Point3d` أو أي نوع Autodesk.

### 2. Autodesk adapter

إنشاء أمر:

```text
GEOINFO
```

السلوك:

1. يطلب اختيار كائن أو نقطة من الرسم.
2. يقرأ نقطة الكائن من خلال Adapter منفصل.
3. يقرأ Drawing CRS من إعدادات Phase 2، أو يطلبه صراحة إذا لم يكن محفوظًا.
4. يعرض نافذة WPF تحتوي على:
   - X/Y/Z الأصلي.
   - Easting/Northing.
   - Longitude/Latitude عند توفر تحويل صالح.
   - Elevation.
   - Source CRS.
   - Drawing CRS.
   - EPSG.
   - Axis order.
   - Units.
   - Datum/ellipsoid عندما تتوفر من تعريف CRS.
   - رسالة تحذير واضحة عند عدم وجود CRS أو PROJ.
5. يوفر زر نسخ الإحداثيات إلى Clipboard.
6. يسجل العملية والنتيجة في log.

### 3. Supported AutoCAD geometry in the first increment

- `DBPoint`.
- `Polyline` vertices.
- `Line` start/end.
- `Circle` center.

الكائنات غير المدعومة لا تفشل بصمت؛ تظهر رسالة توضح نوع الكائن والإجراء المقترح.

### 4. CRS behavior

- لا يتم افتراض WGS84 أو UTM أو CRS يمني تلقائيًا.
- إذا كان CRS الرسم فارغًا، يعرض `GEOINFO` حالة `CRS not configured` ويمنع التحويل.
- إذا اختلف Source CRS عن Target CRS، يجب استخدام PROJ من خلال `GEOLOCAL_PROJ_CS2CS`.
- إذا لم يكن PROJ متاحًا، تعرض الواجهة سبب الفشل ولا تعرض قيمة محولة.
- كل نتيجة تعرض Source → Target حتى لا يحدث التباس بين إحداثيات الرسم والإحداثيات الجغرافية.

### 5. Units and axis order

إضافة طبقة metadata من PROJ/ملف CRS، مع إبقاء التحويل الرياضي في PROJ. يمنع Phase 4 كتابة معادلات UTM أو تحويل feet/meters داخل الواجهة دون تعريف الوحدة.

يجب أن تميز الاختبارات بين:

- International foot.
- US survey foot.
- Meter.
- Degree.
- Radian عند الحاجة.

## خطة التنفيذ المرحلية

| Increment | المخرجات | معيار الإيقاف |
|---|---|---|
| 4.0 | Core contracts وvalidation | اختبارات Core تمر دون Autodesk |
| 4.1 | Autodesk point adapter و`GEOINFO` الأساسي | `DBPoint` يعرض X/Y/Z الصحيح |
| 4.2 | WPF inspector وCopy | نافذة عملية ورسائل خطأ قابلة للفهم |
| 4.3 | CRS/PROJ integration | اختبار 4326↔32638 مع مصدر/هدف ظاهرين |
| 4.4 | Line/Polyline/Circle | كل نوع مدعوم له اختبار مستقل |
| 4.5 | Civil 3D validation | اختبار NETLOAD وGEOINFO موثق داخل Civil 3D 2020 |

## الاختبارات المطلوبة

### Unit tests

- Coordinate validation.
- CRS missing/invalid.
- EPSG canonicalization.
- Source equals target: no external transformation.
- WGS84 → UTM 38N and reverse with known tolerance.
- International foot vs US survey foot is never treated as identical.
- NaN/Infinity and malformed PROJ output.
- Clipboard formatting uses invariant decimal separator.

### Integration tests in Civil 3D 2020

- NETLOAD loads all managed GeoLocalCAD assemblies.
- `GEOINFO` recognizes a DBPoint.
- `GEOINFO` reads a Line and Polyline without modifying the drawing.
- Missing Drawing CRS produces a visible blocking warning.
- Invalid PROJ path produces an actionable error and a log entry.
- Copy button places the displayed coordinate string on the clipboard.

## معايير قبول Phase 4

Phase 4 لا تعتبر مكتملة إلا عند تحقق كل التالي:

- `GEOINFO` command is registered and executes inside Civil 3D 2020.
- Core and CRS tests pass.
- At least DBPoint, Line, and Polyline are tested in Civil 3D.
- Source and target CRS are always visible.
- No silent transformation or geographic assumption exists.
- Log contains the selected entity type, CRS source/target, success/failure, and exception details.
- Windows build produces `GeoLocalCAD.dll`, `GeoLocalCAD.Core.dll`, `GeoLocalCAD.UI.dll`, `GeoLocalCAD.CRS.dll`, and `GeoLocalCAD.Vector.dll` without Autodesk DLLs in the package.
- `README-INSTALL.md` and this validation record are updated with actual evidence.

## مخاطر تقنية وإجراءات تخفيفها

- **محور EPSG الجغرافي:** يستخدم PROJ مع سياسة محور موثقة؛ لا تُكتب معادلات محلية.
- **CRS غير محفوظ في DWG:** يطلبه المستخدم صراحة ولا يخمنه التطبيق.
- **DLL version mismatch:** يبني المشروع مقابل DLLs المثبتة من Civil 3D 2020 فقط، ولا يخلط إصدارات AutoCAD.
- **WPF داخل AutoCAD:** تستخدم نافذة Modal من Autodesk Application API ولا يتم إنشاء UI من thread غير مناسب.
- **كائنات Civil 3D المتخصصة:** تبقى خارج أول Increment وتحتاج Adapter واختبارًا منفصلًا.
