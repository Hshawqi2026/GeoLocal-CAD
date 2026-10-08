# GeoLocal CAD — Production v1.0.0 Roadmap

## القرار

النطاق المعتمد هو النسخة الإنتاجية الكاملة المحددة في `GeoLocalCADx.txt`، وليس إصدارًا نهائيًا شكليًا مبنيًا على الكود الحالي. لذلك لا يُعلن `v1.0.0` إلا بعد تنفيذ Phases 5–12، بناء Release حقيقي، واختبار الأوامر داخل Civil 3D 2020 على Windows مرخص.

الإصدارات الحالية `v0.1.0` إلى `v0.4.0` هي إصدارات مرحلية/source-validation. الإصدار `v1.0.0` يحتاج دليل قبول مستقل ونتيجة اختبار Civil 3D فعلية.

## نطاق المنتج النهائي

يوفر GeoLocal CAD إضافة Offline-first داخل AutoCAD Civil 3D 2020، مع CRS وتحويلات حقيقية، Raster وVector، Georeferencing، DEM وContours، تكامل Civil 3D، وإدارة مصادر محلية وTile Cache دون Web APIs أثناء وضع Offline.

## خطة الإصدارات الإلزامية

| المرحلة | الإصدار المستهدف | المخرجات الأساسية | بوابة القبول |
|---|---:|---|---|
| Phase 5 | v0.5.0 | Raster/GeoTIFF/World Files وRaster layer | قراءة metadata وموضع حقيقي واختبار دون `(0,0)` افتراضي |
| Phase 6 | v0.6.0 | Shapefile وGeoPackage وGeoJSON وDXF adapters | Geometry/Attributes/CRS وRound-trip |
| Phase 7 | v0.7.0 | Similarity/Affine/Helmert وControl Points | RMSE وResiduals ورفض الحل السيئ |
| Phase 8 | v0.8.0 | DEM وارتفاعات وContours ونقاط ارتفاع | NoData وارتفاعات مرتبطة بالموقع الحقيقي |
| Phase 9 | v0.9.0 | COGO/TIN/Feature Lines/Alignments/Profiles | Civil 3D integration داخل نسخة 2020 |
| Phase 10 | v0.10.0 | MBTiles/GeoPackage/Raster tiles وOffline map view | لا اتصال شبكي أثناء Offline |
| Phase 11 | v0.11.0 | Regression/performance/security/reliability | مصفوفة اختبار كاملة وتقرير أداء |
| Phase 12 | v1.0.0 | Installer/ZIP/Documentation/Release | Windows Civil 3D acceptance evidence |

## Phase 5 — Raster Engine

ينشأ `GeoLocalCAD.Raster` بعقود مستقلة عن Autodesk. يجب أن تدعم القراءة الحقيقية لـ GeoTIFF وTIFF/JPEG/PNG/BMP مع World Files مثل JGW/TFW/PGW، وقراءة Pixel Size وRotation وOrigin وExtent وResolution وNoData وBands وCRS.

مخرجات التطبيق تكون Raster layer أو Image entity/adapter مرتبطة بالإحداثيات الفعلية. لا يسمح الكود بوضع الصورة عند `(0,0)` إلا إذا كانت الـ geotransform تقرر ذلك. يستخدم GDAL أو binding موثقًا وموزعًا وفق ترخيصه، مع حصر native DLLs القابلة لإعادة التوزيع داخل الحزمة وعدم تضمين Autodesk DLLs.

الأوامر: `GEOIMPORTRASTER` أو واجهة واضحة تحت Maps/Raster، مع فحص CRS وتحويله صراحة. الاختبارات تشمل World File rotation، Pixel-to-world، NoData، CRS mismatch، ورفض Raster غير georeferenced دون تأكيد صريح من المستخدم.

## Phase 6 — Vector Engine

توسيع `GeoLocalCAD.Vector` إلى Shapefile وGeoPackage وDXF adapters مع حفظ Geometry وAttributes وCRS وLayers. كل adapter يملك reader/writer فعليًا، schema validation، encoding handling، وRound-trip tests. يظل GeoJSON مدعومًا للتوافق مع Phase 3.

ينشأ Storage boundary لربط feature attributes بـ AutoCAD entities دون فقدان المصدر. يجب أن يكون لكل import تقرير بعدد العناصر المستوردة والمتخطاة وأسباب التخطي، مع Layer policy حتمية.

الأوامر: `GEOIMPORT` و`GEOEXPORT` يتوسعان بصيغة يحددها المستخدم أو الامتداد، مع `GEOREPROJECT` لتحويل مجموعة بيانات كاملة بين CRS صريحين.

## Phase 7 — Georeferencing

ينشأ `GeoLocalCAD.Georeferencing` ويحتوي Control Point model، Source/Target coordinates، Similarity transformation، Affine transformation، وHelmert عند الحاجة. يجب حساب RMSE وResidual لكل نقطة، وكتابة Quality Report قبل التطبيق.

الأمر `GEOREF` يتيح تحديد نقاط التحكم، إدخال الإحداثيات، حساب الحل، عرض residuals، قبول/رفض النتيجة، ثم تطبيق التحويل على نسخة أو على entities بعد تأكيد واضح. لا يطبق التحويل إذا كان عدد النقاط غير كافٍ أو RMSE خارج tolerance يحدده المستخدم.

الاختبارات تشمل حلولًا معروفة، نقاط collinear، singular matrix، outlier، وحدود التفاوت.

## Phase 8 — DEM وContours

ينشأ `GeoLocalCAD.DEM` فوق Raster contracts. يدعم Import DEM، قراءة elevation، NoData، استخراج الارتفاع، إنشاء elevation points، contour generation، وterrain analysis. ينفذ `GEODEM` و`GEOCONTOUR` بعمليات حقيقية، مع خيار إنشاء Civil 3D Surface مؤجل إلى Phase 9.

يجب تثبيت سياسة الوحدات الرأسية والأفقية، وتسجيل vertical datum إن كان معروفًا. Contours لا تُنشأ من قيم NoData، وكل ناتج يحمل مصدر DEM وCRS وinterval داخل التقرير.

## Phase 9 — Civil 3D Integration

ينشأ `GeoLocalCAD.Civil3D` كـ Autodesk-only adapter. يدعم تدريجيًا COGO Points وTIN Surface وFeature Lines وAlignments وProfiles وSurvey-related objects. كل نوع يستخدم version-specific adapter لـ Civil 3D 2020 ولا يسرّب أنواع Autodesk إلى Core.

الأمر `GEOADDPNT` ينشئ COGO points من مصدر Vector/DEM مع Description وPoint Group وLayer policy. Surface creation يضيف points/breaklines/boundary وفق transaction آمنة، ويعرض عدد العناصر والنتيجة.

لا يعتبر أي Civil 3D type مكتملًا إلا بعد اختبار NETLOAD وفتح DWG وإعادة فتحه والتحقق من الكائنات والـ units.

## Phase 10 — Offline Maps وTile Cache

ينشأ `GeoLocalCAD.Storage` مع SQLite وMBTiles/GeoPackage local sources وTile index وZoom levels وCRS metadata وcache management. يدعم `GEOTILE` عرض المصادر المحلية فقط في Offline mode.

يمنع HTTP/HTTPS والـ Web APIs عند `OfflineMode=true`. يجب أن يملك الاختبار Network Guard يثبت عدم وجود outbound request أثناء القراءة والعرض. مصادر مثل Google/Bing/ArcGIS/Mapbox/OSM online لا تدخل ضمن baseline.

## Phase 11 — Testing وOptimization

تنشأ مشاريع tests مستقلة لـ Core/CRS/Raster/Vector/Georeferencing/DEM/Storage، بالإضافة إلى IntegrationTests تعمل على Windows/Civil 3D 2020. تضاف Regression Test لكل bug.

يُقاس زمن import/export، استهلاك الذاكرة، عدد الكيانات، حجم Raster، سرعة التحويل، وفشل العمليات الكبيرة. يجب أن تكون المعاملات قابلة للإلغاء أو الفشل الآمن، وأن لا تترك رسمًا نصف مستوردًا.

يتم تدقيق native dependencies والتراخيص وMark-of-the-Web وTrusted Locations، ويُفحص ZIP لاستبعاد Autodesk DLLs وعدم وجود أسرار أو بيانات مستخدم.

## Phase 12 — Production Release

يُبنى Release من Windows clean build environment مقابل Autodesk APIs المثبتة في Civil 3D 2020. الحزمة تحتوي على GeoLocal assemblies وnative dependencies القابلة لإعادة التوزيع وconfiguration و`README-INSTALL.md` و`CHANGELOG.md` و`LICENSE`، ولا تحتوي Autodesk DLLs.

يُنشأ:

```text
GeoLocalCAD_v1.0.0.zip
```

ويشمل تقرير قبول يذكر Windows version وCivil 3D build وPROJ/GDAL versions، كل الأوامر، ملفات الاختبار، النتائج، والقيود المتبقية.

## أوامر النسخة الإنتاجية

```text
GEOLOCAL
GEOCRS
GEOINFO
GEOIMPORT
GEOEXPORT
GEOREF
GEOREPROJECT
GEODEM
GEOCONTOUR
GEOTILE
GEOSETTINGS
GEOADDPNT
```

لا يضاف أمر إلى قائمة النسخة النهائية إلا إذا كان حقيقيًا، موثقًا، ومختبرًا؛ لا توجد dummy commands أو رسائل نجاح بديلة عن التنفيذ.

## بوابة عدم الادعاء بالاكتمال

لا يصدر `v1.0.0` إذا بقي أي مما يلي: Build غير ناجح، dependency غير محلولة، command غير مختبر داخل Civil 3D، Raster أو Vector يوضع في إحداثيات افتراضية، CRS صامت، native license غير موثق، Autodesk DLL داخل الحزمة، أو غياب test evidence.

## الاعتماديات والتراخيص

PROJ وGDAL/OGR وGEOS وSQLite وMBTiles libraries يجب أن تكون إصدارات محددة ومذكورة مع تراخيصها وملاحظات إعادة التوزيع. Autodesk Managed APIs تبقى dependency محلية مرخصة ولا توضع في GitHub أو ZIP.
