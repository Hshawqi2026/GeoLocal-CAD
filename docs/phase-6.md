# Phase 6 — Vector + Raster-to-DEM Surface Bridge

## الهدف

توسيع Phase 6 بحيث لا تقتصر على Vector adapters، بل توفر مسارًا مضبوطًا لتحويل Raster georeferenced المستورد في Phase 5 إلى عينات ارتفاع مرتبطة بالإحداثيات الحقيقية، ثم تمريرها إلى Civil 3D Adapter لإنشاء TIN Surface.

## التصميم

```text
Raster/GeoTIFF
  -> RasterMetadata + GeoTransform
  -> pixel elevation sampler
  -> DemGrid
  -> valid DemSample list
  -> Civil3D IDemSurfaceBuilder
  -> TinSurface
```

يحتوي `GeoLocalCAD.DEM` على عقود مستقلة عن Autodesk. `DemGrid` يحتفظ بمصفوفة الارتفاعات، GeoTransform، CRS، وNoData. يعيد `EnumerateValidSamples()` مراكز pixels فقط عندما تكون قيمة الارتفاع رقمية وليست NoData، ويضعها في world coordinates من خلال GeoTransform. لا يسمح النموذج بإنشاء Grid بلا CRS أو georeferencing.

`Civil3DSurfaceRequest` يحمل اسم Surface وLayer وquality tolerance. أما `IDemSurfaceBuilder` فهو contract للطبقة الخاصة بـ Civil 3D؛ التطبيق الفعلي سيستخدم Civil 3D 2020 API لإنشاء TinSurface وإضافة elevation points داخل Transaction آمنة.

## علاقة Phase 5

`GEOIMPORTRASTER` في Phase 5 يضع الصورة في موقعها الجغرافي، لكن إنشاء DEM يحتاج قراءة قيم الارتفاع من Raster، وليس أبعاد الصورة فقط. لذلك يجب أن يوفر تنفيذ Phase 6/8 pixel sampler عبر GDAL أو binding مرخص، مع توثيق DataType وNoData وvertical units. لا يجوز استخدام ألوان الصورة كارتفاعات أو تخمين الارتفاع من RGB.

## مراحل التنفيذ

| Increment | التنفيذ | معيار القبول |
|---|---|---|
| 6.0 | عقود `DemGrid` و`DemSample` و`Civil3DSurfaceRequest` | Build مستقل واختبار CRS/NoData/Pixel-to-world |
| 6.1 | GDAL raster sampler لـ GeoTIFF DEM | قيمة ارتفاع معروفة وNoData صحيح |
| 6.2 | Civil 3D 2020 adapter لـ TinSurface | Surface حقيقي بعد NETLOAD، دون Autodesk DLLs في الحزمة |
| 6.3 | ربط Vector boundaries وLayer policy | Surface boundary وLayer حتميان |
| 6.4 | تقارير وRollback وPerformance | فشل آمن، log، وعدم ترك Surface جزئي |

## الأوامر

الأمر النهائي سيكون `GEODEM` بعد اكتمال sampler والـ Civil 3D adapter. في Increment 6.0 لا يُنشأ أمر شكلي؛ العقود والاختبارات فقط. بعد 6.2 يطلب `GEODEM` مصدر Raster، CRS الرسم، اسم Surface، Layer، NoData policy، وsampling/decimation options، ثم يعرض preview للعدد المتوقع قبل الإنشاء.

## قواعد CRS والوحدات

Source CRS من GeoTIFF أو من المستخدم بوضوح، وDrawing CRS من إعدادات الرسم. عند اختلافهما يستخدم PROJ تحويلًا صريحًا ولا يستمر عند غياب PROJ. يجب تثبيت horizontal units وvertical units وvertical datum إن توفر؛ لا يعامل foot الدولي وUS survey foot كأنهما متساويان.

## اختبار Civil 3D المطلوب

على Windows مع Civil 3D 2020:

1. تحميل `GeoLocalCAD.dll` عبر `NETLOAD`.
2. تشغيل `GEODEM` على GeoTIFF DEM معروف، مع Source CRS وDrawing CRS صريحين.
3. التحقق من إنشاء `TinSurface` في الموقع الحقيقي، وليس `(0,0)`.
4. التحقق من عدد العينات المقبولة والمستبعدة بسبب NoData.
5. فتح Surface Properties ومراجعة elevations وunits وCRS metadata/log.
6. حفظ DWG وإعادة فتحه والتحقق من بقاء Surface.
7. تجربة Raster بلا CRS أو بلا geotransform والتأكد من الرفض دون إنشاء كائن جزئي.
8. تجربة قيمة vertical unit مختلفة والتأكد من ظهور التحذير أو التحويل الصريح.

## الحالة الحالية

تم تنفيذ عقود `GeoLocalCAD.DEM` واختبارها مستقلًا. لم يُدّعَ بعد تنفيذ GDAL pixel sampler أو Civil 3D TinSurface adapter، لأن ذلك يتطلب binding/Native dependencies واختبارًا على Windows Civil 3D 2020. هذه العناصر هي increments التالية في Phase 6، بينما Phase 8 سيضيف contours والتحليل terrain المتقدم.
