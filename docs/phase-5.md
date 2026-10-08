# Phase 5 — Raster Engine

## Implemented

أضيف مشروع `GeoLocalCAD.Raster` بعقود مستقلة عن Autodesk. يقرأ قارئ metadata حقيقي أبعاد وخصائص PNG وJPEG وBMP وTIFF/GeoTIFF، ويقرأ World Files ذات الصيغ PGW وJGW وTFW وWLD وWORLD.

يتضمن metadata:

- Width وHeight.
- Bands.
- Format وDataType.
- Pixel size.
- Rotation.
- Origin.
- GeoTransform.
- Extent.
- CRS عند وجود EPSG GeoKey أو PRJ authority.
- NoData contract محفوظ في النموذج للربط مع GDAL في المرحلة التالية من Raster sampling.

تنفذ `RasterGeoTransform` تحويلات Pixel-to-world وWorld-to-pixel وتحسب extent من الزوايا الأربع، بما في ذلك raster rotated. World File center coordinates تتحول إلى corner origin وفق معيار world files؛ لا يتم وضع الصورة افتراضيًا عند `(0,0)`.

أضيف أمر AutoCAD:

```text
GEOIMPORTRASTER
```

يقرأ metadata، يرفض raster غير georeferenced، وينشئ `RasterImage` مع `RasterImageDef` واتجاه مرتبط بامتداد raster الحقيقي. لا ينفذ استيرادًا عند غياب GeoTIFF geotransform أو World File.

## الاختبارات المنفذة في بيئة Linux

- بناء `GeoLocalCAD.Raster`.
- PNG metadata dimensions.
- PGW six-line parsing.
- Pixel-to-world expected corner.
- World-to-pixel inverse.
- Extent calculation.
- Regression tests السابقة لـ CRS وGeoJSON وCoordinate Inspector.

النتيجة:

```text
Phase 5 Raster smoke test passed.
```

## Windows/Civil 3D acceptance

- [ ] Build على Windows مقابل Civil 3D 2020 managed APIs.
- [ ] NETLOAD `GeoLocalCAD.dll`.
- [ ] تشغيل `GEOIMPORTRASTER` على GeoTIFF georeferenced.
- [ ] التحقق من أن RasterImage يظهر في extent الحقيقي وليس `(0,0)`.
- [ ] اختبار TIFF/PNG/JPEG/BMP مع World Files.
- [ ] اختبار rotated World File.
- [ ] اختبار Raster غير georeferenced والتأكد من رفضه برسالة واضحة.
- [ ] التحقق من log واسم الملف والـ geotransform والامتداد.

## حدود المرحلة

قارئ metadata المدار يدعم خصائص TIFF/GeoTIFF الأساسية، بينما فك ضغط Raster الكامل وقراءة NoData/Bands على مستوى البكسل عبر GDAL/OGR سيُعززان في إصدار Raster لاحق قبل Phase 8 DEM. لم يتم ادعاء نجاح NETLOAD أو إدراج RasterImage داخل Civil 3D من بيئة Linux.
