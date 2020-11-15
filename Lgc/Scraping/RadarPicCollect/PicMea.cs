//using AsLink;
using AAV.Sys.Ext;
using AsLink;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace RadarPicCollect
{
    public static class PicMea
    {

        public static double CalcMphInTheArea(Bitmap bmp, DateTime imgTime, int radiusInPixels = 40)
        {
            if (bmp == null) return -1;

            var sw = Stopwatch.StartNew();
            try
            {
                int ttlArea = 0, ttlMPH = 0;                                                    //var hmAry = new Color[] { bmp.GetPixel(249, 259), bmp.GetPixel(250, 259), bmp.GetPixel(249, 260), bmp.GetPixel(250, 260) };
                var bkgr = Color.FromArgb(255, 153, 153, 102);              //var hml = new List<Color>();
                var mmcnt = new Dictionary<int, int>();
                for (int x0 = 250, x = -radiusInPixels; x < radiusInPixels; x++)
                {
                    for (int y0 = 260, y = -radiusInPixels; y < radiusInPixels; y++)
                    {
                        ttlArea++;
                        Color pc = bmp.GetPixel(x0 + x, y0 + y);
                        switch (pc.ToArgb())
                        {
                            default: break; //dev: addIfN(mmcnt, -003); Debug.WriteLine(string.Format("case {0}: ttlMPH += 0; break; // {1}, RGB: {2:###} {3:###} {4:###}  => .", pc.ToArgb(), pc.Name, pc.R, pc.G, pc.B)); break;

                            case -10092391: addIfN(mmcnt, 2000); ttlMPH += 2000; break; // ff660099, 660099   RGB: 102  153  => .
                            case -06736948: addIfN(mmcnt, 1500); ttlMPH += 1500; break; // ff9933cc, 9933cc   RGB: 153 51 204  => .
                            case -00064871: addIfN(mmcnt, 1000); ttlMPH += 1000; break; // ffff0299, ff0299   RGB: 255 2 153  => .
                            case -00065536: addIfN(mmcnt, 0750); ttlMPH += 0750; break; // ffff0000, ff0000   RGB: 255    => .
                            case -00039424: addIfN(mmcnt, 0500); ttlMPH += 0500; break; // FFFF6600,    ???   RGB: 51 51 102  => .
                            case -00026368: addIfN(mmcnt, 0360); ttlMPH += 0360; break; // ffff9900, ff9900   RGB: 255 153   => .
                            case -00013312: addIfN(mmcnt, 0240); ttlMPH += 0240; break; // ffffcc00, ffcc00   RGB: 255 204   0  => .
                            case -00000205: addIfN(mmcnt, 0180); ttlMPH += 0180; break; // ffffff33, ffff33   RGB: 255 255  51  => .
                            case -16751104: addIfN(mmcnt, 0120); ttlMPH += 0120; break; // ff006600, 006600   RGB:   0 102   0  => .
                            case -16738048: addIfN(mmcnt, 0080); ttlMPH += 0080; break; // ff009900, 009900   RGB:   0 153   0  => .
                            case -16724992: addIfN(mmcnt, 0040); ttlMPH += 0040; break; // ff00cc00, 00cc00   RGB:   0 204   0  => .
                            case -16711834: addIfN(mmcnt, 0020); ttlMPH += 0020; break; // ff00ff66, 00ff66   RGB:   0 255 102  => .
                            case -16737793: addIfN(mmcnt, 0010); ttlMPH += 0010; break; // ff0099ff, 0099ff   RGB:   0 153 255  => .
                            case -06697729: addIfN(mmcnt, 0001); ttlMPH += 0001; break; // ff99ccff, 99ccff   RGB: 153 204 255  => .
                            case -13421722:                                            // ff333366, RGB:  51  51 102  => Lake
                            case -00000001:                                            // ffffffff, RGB: 255 255 255  => Cross
                            case -06710938: addIfN(mmcnt, 0000); break;                 // ff999966, RGB: 153 153 102  => Land
                        }
                    }
                }

                //int ttl = 0;
                //ttl += dbgShow(mmcnt, 2000);
                //ttl += dbgShow(mmcnt, 1500);
                //ttl += dbgShow(mmcnt, 1000);
                //ttl += dbgShow(mmcnt, 0750);
                //ttl += dbgShow(mmcnt, 0500);
                //ttl += dbgShow(mmcnt, 0360);
                //ttl += dbgShow(mmcnt, 0240);
                //ttl += dbgShow(mmcnt, 0180);
                //ttl += dbgShow(mmcnt, 0120);
                //ttl += dbgShow(mmcnt, 0080);
                //ttl += dbgShow(mmcnt, 0040);
                //ttl += dbgShow(mmcnt, 0020);
                //ttl += dbgShow(mmcnt, 0010);
                //ttl += dbgShow(mmcnt, 0001);
                //ttl += dbgShow(mmcnt, 0000);

                var rv = (double)ttlMPH * .1 / ttlArea;

                //77 Debug.WriteLine(":> {0:HH:mm} {1,5:N1} ms for {2} radiusInPixels => {3} area in pixels  ===> {4:N5} mm/h/km²", imgTime, sw.Elapsed.TotalMilliseconds, radiusInPixels, ttlArea, rv);

                return rv;
            }
            catch (Exception ex) { ex.Log(); return -2; }
        }

        static void addIfN(Dictionary<int, int> mmcnt, int k) { if (mmcnt.ContainsKey(k)) mmcnt[k]++; else mmcnt.Add(k, 1); }
        static int dbgShow(Dictionary<int, int> mmcnt, int k) { return mmcnt.ContainsKey(k) ? mmcnt[k] : 0; }           //	Debug.WriteLine(mmcnt.ContainsKey(k) ? mmcnt[k] : 0, k.ToString("0####"));
    }
}
