﻿using Celeste.Mod.Entities;
using Celeste.Pico8;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Celeste.Mod.PuzzleIslandHelper.Cal
{
    /// <summary>
    /// A copy of the pico8 emulator from vanilla loaded with data for CalJr.
    /// </summary>
    public class CalEmu : Scene
    {
       
        public const string MapData = "2331252548252532323232323300002425262425252631323232252628282824252525252525323328382828312525253232323233000000313232323232323232330000002432323233313232322525252525482525252525252526282824252548252525262828282824254825252526282828283132323225482525252525\r\n252331323232332900002829000000242526313232332828002824262a102824254825252526002a2828292810244825282828290000000028282900000000002810000000372829000000002a2831482525252525482525323232332828242525254825323338282a283132252548252628382828282a2a2831323232322525\r\n252523201028380000002a0000003d24252523201028292900282426003a382425252548253300002900002a0031252528382900003a676838280000000000003828393e003a2800000000000028002425253232323232332122222328282425252532332828282900002a283132252526282828282900002a28282838282448\r\n3232332828282900000000003f2020244825262828290000002a243300002a2425322525260000000000000000003125290000000021222328280000000000002a2828343536290000000000002839242526212223202123313232332828242548262b000000000000001c00003b242526282828000000000028282828282425\r\n2340283828293a2839000000343522252548262900000000000030000000002433003125333d3f00000000000000003100001c3a3a31252620283900000000000010282828290000000011113a2828313233242526103133202828282838242525262b000000000000000000003b2425262a2828670016002a28283828282425\r\n263a282828102829000000000000312525323300000000110000370000003e2400000037212223000000000000000000395868282828242628290000000000002a2828290000000000002123283828292828313233282829002a002a2828242525332b0c00000011110000000c3b314826112810000000006828282828282425\r\n252235353628280000000000003a282426003d003a3900270000000000002125001a000024252611111111000000002c28382828283831332800000017170000002a000000001111000024261028290028281b1b1b282800000000002a2125482628390000003b34362b000000002824252328283a67003a28282829002a3132\r\n25333828282900000000000000283824252320201029003039000000005824480000003a31323235353536675800003c282828281028212329000000000000000000000000003436003a2426282800003828390000002a29000000000031323226101000000000282839000000002a2425332828283800282828390000001700\r\n2600002a28000000003a283a2828282425252223283900372858390068283132000000282828282820202828283921222829002a28282426000000000000000000000000000020382828312523000000282828290000000000163a67682828003338280b00000010382800000b00003133282828282868282828280000001700\r\n330000002867580000281028283422252525482628286720282828382828212200003a283828102900002a28382824252a0000002838242600000017170000000000000000002728282a283133390000282900000000000000002a28282829002a2839000000002a282900000000000028282838282828282828290000000000\r\n0000003a2828383e3a2828283828242548252526002a282729002a28283432250000002a282828000000002810282425000000002a282426000000000000000000000000000037280000002a28283900280000003928390000000000282800000028290000002a2828000000000000002a282828281028282828675800000000\r\n0000002838282821232800002a28242532322526003a2830000000002a28282400000000002a281111111128282824480000003a28283133000000000000171700013f0000002029000000003828000028013a28281028580000003a28290000002a280c0000003a380c00000000000c00002a2828282828292828290000003a\r\n00013a2123282a313329001111112425002831263a3829300000000000002a310000000000002834222236292a0024253e013a3828292a00000000000000000035353536000020000000003d2a28671422222328282828283900582838283d00003a290000000028280000000000000000002a28282a29000058100012002a28\r\n22222225262900212311112122222525002a3837282900301111110000003a2800013f0000002a282426290000002425222222232900000000000000171700002a282039003a2000003a003435353535252525222222232828282810282821220b10000000000b28100000000b0000002c00002838000000002a283917000028\r\n2548252526111124252222252525482500012a2828673f242222230000003828222223000012002a24260000001224252525252600000000171700000000000000382028392827080028676820282828254825252525262a28282122222225253a28013d0000006828390000000000003c0168282800171717003a2800003a28\r\n25252525252222252525252525252525222222222222222525482667586828282548260000270000242600000021252525254826171700000000000000000000002a2028102830003a282828202828282525252548252600002a2425252548252821222300000028282800000000000022222223286700000000282839002838\r\n2532330000002432323232323232252525252628282828242532323232254825253232323232323225262828282448252525253300000000000000000000005225253232323233313232323233282900262829286700000000002828313232322525253233282800312525482525254825254826283828313232323232322548\r\n26282800000030402a282828282824252548262838282831333828290031322526280000163a28283133282838242525482526000000000000000000000000522526000016000000002a10282838390026281a3820393d000000002a3828282825252628282829003b2425323232323232323233282828282828102828203125\r\n3328390000003700002a3828002a2425252526282828282028292a0000002a313328111111282828000028002a312525252526000000000000000000000000522526000000001111000000292a28290026283a2820102011111121222328281025252628382800003b24262b002a2a38282828282829002a2800282838282831\r\n28281029000000000000282839002448252526282900282067000000000000003810212223283829003a1029002a242532323367000000000000000000004200252639000000212300000000002122222522222321222321222324482628282832323328282800003b31332b00000028102829000000000029002a2828282900\r\n2828280016000000162a2828280024252525262700002a2029000000000000002834252533292a0000002a00111124252223282800002c46472c00000042535325262800003a242600001600002425252525482631323331323324252620283822222328292867000028290000000000283800111100001200000028292a1600\r\n283828000000000000003a28290024254825263700000029000000000000003a293b2426283900000000003b212225252526382867003c56573c4243435363633233283900282426111111111124252525482526201b1b1b1b1b24252628282825252600002a28143a2900000000000028293b21230000170000112867000000\r\n2828286758000000586828380000313232323320000000000000000000272828003b2426290000000000003b312548252533282828392122222352535364000029002a28382831323535353522254825252525252300000000003132332810284825261111113435361111111100000000003b3133111111111127282900003b\r\n2828282810290000002a28286700002835353536111100000000000011302838003b3133000000000000002a28313225262a282810282425252662636400000000160028282829000000000031322525252525252667580000002000002a28282525323535352222222222353639000000003b34353535353536303800000017\r\n282900002a0000000000382a29003a282828283436200000000000002030282800002a29000011110000000028282831260029002a282448252523000000000039003a282900000000000000002831322525482526382900000017000058682832331028293b2448252526282828000000003b201b1b1b1b1b1b302800000017\r\n283a0000000000000000280000002828283810292a000000000000002a3710281111111111112136000000002a28380b2600000000212525252526001c0000002828281000000000001100002a382829252525252628000000001700002a212228282908003b242525482628282912000000001b00000000000030290000003b\r\n3829000000000000003a102900002838282828000000000000000000002a2828223535353535330000000000002828393300000000313225252533000000000028382829000000003b202b00682828003232323233290000000000000000312528280000003b3132322526382800170000000000000000110000370000000000\r\n290000000000000000002a000000282928292a0000000000000000000000282a332838282829000000000000001028280000000042434424252628390000000028002a0000110000001b002a2010292c1b1b1b1b0000000000000000000010312829160000001b1b1b313328106700000000001100003a2700001b0000000000\r\n00000100000011111100000000002a3a2a0000000000000000000000002a2800282829002a000000000000000028282800000000525354244826282800000000290000003b202b39000000002900003c000000000000000000000000000028282800000000000000001b1b2a2829000001000027390038300000000000000000\r\n1111201111112122230000001212002a00010000000000000000000000002900290000000000000000002a6768282900003f01005253542425262810673a3900013f0000002a3829001100000000002101000000000000003a67000000002a382867586800000100000000682800000021230037282928300000000000000000\r\n22222222222324482611111120201111002739000017170000001717000000000001000000001717000000282838393a0021222352535424253328282838290022232b00000828393b27000000001424230000001200000028290000000000282828102867001717171717282839000031333927101228370000000000000000\r\n254825252526242526212222222222223a303800000000000000000000000000001717000000000000003a28282828280024252652535424262828282828283925262b00003a28103b30000000212225260000002700003a28000000000000282838282828390000005868283828000022233830281728270000000000000000\r\n00000000000000008242525252528452339200001323232352232323232352230000000000000000b302000013232352526200a2828342525223232323232323\r\n00000000000000a20182920013232352363636462535353545550000005525355284525262b20000000000004252525262828282425284525252845252525252\r\n00000000000085868242845252525252b1006100b1b1b1b103b1b1b1b1b103b100000000000000111102000000a282425233000000a213233300009200008392\r\n000000000000110000a2000000a28213000000002636363646550000005525355252528462b2a300000000004252845262828382132323232323232352528452\r\n000000000000a201821323525284525200000000000000007300000000007300000000000000b343536300410000011362b2000000000000000000000000a200\r\n0000000000b302b2002100000000a282000000000000000000560000005526365252522333b28292001111024252525262019200829200000000a28213525252\r\n0000000000000000a2828242525252840000000000000000b10000000000b1000000000000000000b3435363930000b162273737373737373737374711000061\r\n000000110000b100b302b20000006182000000000000000000000000005600005252338282828201a31222225252525262820000a20011111100008283425252\r\n0000000000000093a382824252525252000061000011000000000011000000001100000000000000000000020182001152222222222222222222222232b20000\r\n0000b302b200000000b10000000000a200000000000000009300000000000000846282828283828282132323528452526292000000112434440000a282425284\r\n00000000000000a2828382428452525200000000b302b2936100b302b20061007293a30000000000000000b1a282931252845252525252232323232362b20000\r\n000000b10000001100000000000000000000000093000086820000a3000000005262828201a200a282829200132323236211111111243535450000b312525252\r\n00000000000000008282821323232323820000a300b1a382930000b100000000738283931100000000000011a382821323232323528462829200a20173b20061\r\n000000000000b302b2000061000000000000a385828286828282828293000000526283829200000000a20000000000005222222232263636460000b342525252\r\n00000011111111a3828201b1b1b1b1b182938282930082820000000000000000b100a282721100000000b372828283b122222232132333610000869200000000\r\n00100000000000b1000000000000000086938282828201920000a20182a37686526282829300000000000000000000005252845252328283920000b342845252\r\n00008612222232828382829300000000828282828283829200000000000061001100a382737200000000b373a2829211525284628382a2000000a20000000000\r\n00021111111111111111111111110061828282a28382820000000000828282825262829200000000000000000000000052525252526201a2000000b342525252\r\n00000113235252225353536300000000828300a282828201939300001100000072828292b1039300000000b100a282125223526292000000000000a300000000\r\n0043535353535353535353535363b2008282920082829200061600a3828382a28462000000000000000000000000000052845252526292000011111142525252\r\n0000a28282132362b1b1b1b1000000009200000000a28282828293b372b2000073820100110382a3000000110082821362101333610000000000008293000000\r\n0002828382828202828282828272b20083820000a282d3000717f38282920000526200000000000093000000000000005252525284620000b312223213528452\r\n000000828392b30300000000002100000000000000000082828282b303b20000b1a282837203820193000072a38292b162710000000000009300008382000000\r\n00b1a282820182b1a28283a28273b200828293000082122232122232820000a3233300000000000082920000000000002323232323330000b342525232135252\r\n000000a28200b37300000000a37200000010000000111111118283b373b200a30000828273039200828300738283001162930000000000008200008282920000\r\n0000009261a28200008261008282000001920000000213233342846282243434000000000000000082000085860000008382829200000000b342528452321323\r\n0000100082000082000000a2820300002222321111125353630182829200008300009200b1030000a28200008282001262829200000000a38292008282000000\r\n00858600008282a3828293008292610082001000001222222252525232253535000000f3100000a3820000a2010000008292000000009300b342525252522222\r\n0400122232b200839321008683039300528452222262c000a28282820000a38210000000a3738000008293008292001362820000000000828300a38201000000\r\n00a282828292a2828283828282000000343434344442528452525252622535350000001263000083829300008200c1008210d3e300a38200b342525252845252\r\n1232425262b28682827282820103820052525252846200000082829200008282320000008382930000a28201820000b162839300000000828200828282930000\r\n0000008382000000a28201820000000035353535454252525252528462253535000000032444008282820000829300002222223201828393b342525252525252\r\n525252525262b2b1b1b1132323526200845223232323232352522323233382825252525252525252525284522333b2822323232323526282820000b342525252\r\n52845252525252848452525262838242528452522333828292425223232352520000000000000000000000000000000000000000000000000000000000000000\r\n525252845262b2000000b1b1b142620023338276000000824233b2a282018283525252845252232323235262b1b10083921000a382426283920000b342232323\r\n2323232323232323232323526201821352522333b1b1018241133383828242840000000000000000000000000000000000000000000000000000000000000000\r\n525252525262b20000000000a242627682828392000011a273b200a382729200525252525233b1b1b1b11333000000825353536382426282410000b30382a2a2\r\na1829200a2828382820182426200a2835262b1b10000831232b2000080014252000000000000a300000000000000000000000000000000000000000000000000\r\n528452232333b20000001100824262928201a20000b3720092000000830300002323525262b200000000b3720000a382828283828242522232b200b373928000\r\n000100110092a2829211a2133300a3825262b2000000a21333b20000868242520000000000000100009300000000000000000000000000000000000000000000\r\n525262122232b200a37672b2a24262838292000000b30300000000a3820300002232132333b200000000b303829300a2838292019242845262b2000000000000\r\n00a2b302b2a36182b302b200110000825262b200000000b1b10000a283a2425200000000a30082000083000000000000000000000094a4b4c4d4e4f400000000\r\n525262428462b200a28303b2214262928300000000b3030000000000a203e3415252222232b200000000b30392000000829200000042525262b2000000000000\r\n000000b100a2828200b100b302b211a25262b200000000000000000092b3428400000000827682000001009300000000000000000095a5b5c5d5e5f500000000\r\n232333132362b221008203b2711333008293858693b3031111111111114222225252845262b200001100b303b2000000821111111142528462b2000000000000\r\n000000000000110176851100b1b3026184621111111100000061000000b3135200000000828382670082768200000000000000000096a6b6c6d6e6f600000000\r\n82000000a203117200a203b200010193828283824353235353535353535252845252525262b200b37200b303b2000000824353535323235262b2000011000000\r\n0000000000b30282828372b26100b100525232122232b200000000000000b14200000000a28282123282839200000000000000000097a7b7c7d7e7f700000000\r\n9200110000135362b2001353535353539200a2000001828282829200b34252522323232362b261b30300b3030000000092b1b1b1b1b1b34262b200b372b20000\r\n001100000000b1a2828273b200000000232333132333b200001111000000b342000000868382125252328293a300000000000000000000000000000000000000\r\n00b372b200a28303b2000000a28293b3000000000000a2828382827612525252b1b1b1b173b200b30393b30361000000000000000000b34262b271b303b20000\r\nb302b211000000110092b100000000a3b1b1b1b1b1b10011111232110000b342000000a282125284525232828386000000000000000000000000000000000000\r\n80b303b20000820311111111008283b311111111110000829200928242528452000000a3820000b30382b37300000000000000000000b3426211111103b20000\r\n00b1b302b200b372b200000000000082b21000000000b31222522363b200b3138585868292425252525262018282860000000000000000000000000000000000\r\n00b373b20000a21353535363008292b32222222232111102b20000a21323525200000001839200b3038282820000000011111111930011425222222233b20000\r\n100000b10000b303b200000000858682b27100000000b3425233b1b1000000b182018283001323525284629200a2820000000000000000000000000000000000\r\n9300b100000000b1b1b1b1b100a200b323232323235363b100000000b1b1135200000000820000b30382839200000000222222328283432323232333b2000000\r\n329300000000b373b200000000a20182111111110000b31333b100a30061000000a28293f3123242522333020000820000000000000000000000000000000000\r\n829200001000410000000000000000b39310d30000a28200000000000000824200000086827600b30300a282760000005252526200828200a30182a2006100a3\r\n62820000000000b100000093a382838222222232b20000b1b1000083000000860000122222526213331222328293827600000000000000000000000000000000\r\n017685a31222321111111111002100b322223293000182930000000080a301131000a383829200b373000083920000005284526200a282828283920000000082\r\n62839321000000000000a3828282820152845262b261000093000082a300a3821000135252845222225252523201838200000000000000000000000000000000\r\n828382824252522222222232007100b352526282a38283820000000000838282320001828200000083000082010000005252526271718283820000000000a382\r\n628201729300000000a282828382828252528462b20000a38300a382018283821222324252525252525284525222223200000000000000000000000000000000";

        public Scene ReturnTo;


        public CalGame game;


        public int gameFrame;


        public bool gameActive;


        public float gameDelay;


        public Point bootLevel;


        public bool leaving;


        public bool skipFrame;


        public EventInstance bgSfx;


        public VirtualRenderTarget buffer;


        public Color[] pixels;


        public Vector2 offset;


        public TextMenu pauseMenu;


        public float pauseFade;


        public EventInstance snapshot;


        public MTexture picoBootLogo;


        public byte[] tilemap;


        public MTexture[] sprites;


        public byte[] mask;


        public Color[] colors;


        public Dictionary<Color, int> paletteSwap;


        public MTexture[] font;


        public string fontMap;

        public bool CanPause => pauseMenu == null;

        public bool booting
        {

            get
            {
                return game == null;
            }
        }


        public CalEmu(Scene returnTo, int levelX = 0, int levelY = 0)
        {
            orig_ctor(returnTo, levelX, levelY);
            if (!Everest.Content.TryGet<AssetTypeText>("Pico8Tilemap", out var metadata))
            {
                return;
            }

            string input;
            using (StreamReader streamReader = new StreamReader(metadata.Stream))
            {
                input = streamReader.ReadToEnd();
            }

            input = Regex.Replace(input, "\\s+", "");
            tilemap = new byte[input.Length / 2];
            int length = input.Length;
            int num = length / 2;
            for (int i = 0; i < length; i += 2)
            {
                char c = input[i];
                char c2 = input[i + 1];
                byte[] array = tilemap;
                int num2 = i / 2;
                char reference;
                char reference2;
                string s;
                if (i >= num)
                {
                    reference = c2;
                    ReadOnlySpan<char> readOnlySpan = new ReadOnlySpan<char>(in reference);
                    reference2 = c;
                    s = string.Concat(readOnlySpan, new ReadOnlySpan<char>(in reference2));
                }
                else
                {
                    reference2 = c;
                    ReadOnlySpan<char> readOnlySpan2 = new ReadOnlySpan<char>(in reference2);
                    reference = c2;
                    s = string.Concat(readOnlySpan2, new ReadOnlySpan<char>(in reference));
                }

                array[num2] = (byte)int.Parse(s, NumberStyles.HexNumber);
            }
        }


        public override void Begin()
        {
            bgSfx = Audio.Play("event:/env/amb/03_pico8_closeup");
            base.Begin();
        }


        public override void End()
        {
            buffer.Dispose();
            Audio.BusStopAll("bus:/gameplay_sfx");
            Audio.Stop(bgSfx);
            if (snapshot != null)
            {
                Audio.ReleaseSnapshot(snapshot);
            }

            snapshot = null;
            Stats.Store();
            base.End();
        }



        public void ResetScreen()
        {
            Engine.Graphics.GraphicsDevice.Textures[0] = null;
            Engine.Graphics.GraphicsDevice.Textures[1] = null;
            for (int i = 0; i < 128; i++)
            {
                for (int j = 0; j < 128; j++)
                {
                    pixels[i + j * 128] = Color.Black;
                }
            }

            buffer.Target.SetData(pixels);
        }


        public override void Update()
        {
            base.Update();
            if (pauseMenu != null)
            {
                pauseMenu.Update();
            }
            else if (!leaving && (Input.Pause.Pressed || Input.ESC.Pressed))
            {
                Input.Pause.ConsumeBuffer();
                Input.ESC.ConsumeBuffer();
                CreatePauseMenu();
            }

            pauseFade = Calc.Approach(pauseFade, (pauseMenu != null) ? 0.75f : 0f, Engine.DeltaTime * 6f);
            skipFrame = !skipFrame;
            if (skipFrame)
            {
                return;
            }

            gameDelay -= Engine.DeltaTime;
            if (!gameActive || gameDelay > 0f)
            {
                return;
            }

            if (booting)
            {
                Engine.Graphics.GraphicsDevice.Textures[0] = null;
                Engine.Graphics.GraphicsDevice.Textures[1] = null;
                gameFrame++;
                int num = gameFrame - 20;
                if (num == 1)
                {
                    for (int i = 0; i < 128; i++)
                    {
                        for (int j = 2; j < 128; j += 8)
                        {
                            pixels[j + i * 128] = colors[Calc.Random.Next(4) + i / 32];
                        }
                    }

                    buffer.Target.SetData(pixels);
                }

                if (num == 4)
                {
                    for (int k = 0; k < 128; k += 2)
                    {
                        for (int l = 0; l < 128; l += 4)
                        {
                            pixels[l + k * 128] = colors[6 + (((l + k) / 8) & 7)];
                        }
                    }

                    buffer.Target.SetData(pixels);
                }

                if (num == 7)
                {
                    for (int m = 0; m < 128; m += 3)
                    {
                        for (int n = 2; n < 128; n += 4)
                        {
                            pixels[n + m * 128] = colors[10 + Calc.Random.Next(4)];
                        }
                    }

                    buffer.Target.SetData(pixels);
                }

                if (num == 9)
                {
                    for (int num2 = 0; num2 < 128; num2++)
                    {
                        for (int num3 = 1; num3 < 127; num3 += 2)
                        {
                            pixels[num3 + num2 * 128] = pixels[num3 + 1 + num2 * 128];
                        }
                    }

                    buffer.Target.SetData(pixels);
                }

                if (num == 12)
                {
                    for (int num4 = 0; num4 < 128; num4++)
                    {
                        if ((num4 & 3) > 0)
                        {
                            for (int num5 = 0; num5 < 128; num5++)
                            {
                                pixels[num5 + num4 * 128] = colors[0];
                            }
                        }
                    }

                    buffer.Target.SetData(pixels);
                }

                if (num == 15)
                {
                    for (int num6 = 0; num6 < 128; num6++)
                    {
                        for (int num7 = 0; num7 < 128; num7++)
                        {
                            pixels[num7 + num6 * 128] = colors[0];
                        }
                    }

                    buffer.Target.SetData(pixels);
                }

                if (num == 30)
                {
                    Audio.Play("event:/classic/pico8_boot");
                }

                if (num == 30 || num == 35 || num == 40)
                {
                    Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
                    Engine.Graphics.GraphicsDevice.Clear(colors[0]);
                    Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, RasterizerState.CullNone);
                    picoBootLogo.Draw(new Vector2(1f, 1f));
                    if (num >= 35)
                    {
                        print("pico-8 0.1.9B", 1f, 18f, 6f);
                    }

                    if (num >= 40)
                    {
                        print("(c) 2014-16 lexaloffle games llp", 1f, 24f, 6f);
                        print("booting cartridge..", 1f, 36f, 6f);
                    }

                    Draw.SpriteBatch.End();
                    Engine.Graphics.GraphicsDevice.SetRenderTarget(null);
                }

                if (num == 90)
                {
                    gameFrame = 0;
                    game = new CalGame();
                    game.Init(this);
                    if (bootLevel.X != 0 || bootLevel.Y != 0)
                    {
                        game.load_room(bootLevel.X, bootLevel.Y);
                    }
                }

                return;
            }

            gameFrame++;
            game.Update();
            if (game.freeze > 0)
            {
                return;
            }

            Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
            Engine.Graphics.GraphicsDevice.Clear(colors[0]);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, RasterizerState.CullNone, null, Matrix.CreateTranslation(0f - offset.X, 0f - offset.Y, 0f));
            game.Draw();
            Draw.SpriteBatch.End();
            Engine.Graphics.GraphicsDevice.SetRenderTarget(null);
            if (paletteSwap.Count <= 0)
            {
                return;
            }

            buffer.Target.GetData(pixels);
            for (int num8 = 0; num8 < pixels.Length; num8++)
            {
                int value = 0;
                if (paletteSwap.TryGetValue(pixels[num8], out value))
                {
                    pixels[num8] = colors[value];
                }
            }

            buffer.Target.SetData(pixels);
        }


        public override void Render()
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, RasterizerState.CullNone, null, Engine.ScreenMatrix);
            int num = 6;
            Vector2 vector = new Vector2(buffer.Width * num, buffer.Height * num);
            Vector2 position = new Vector2(1920f - vector.X, 1080f - vector.Y) / 2f;
            bool flag = SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode;
            GFX.Game["pico8/consoleBG"].Draw(Vector2.Zero, Vector2.Zero, Color.White, num);
            Draw.SpriteBatch.Draw((RenderTarget2D)buffer, position, buffer.Bounds, Color.White, 0f, Vector2.Zero, num, flag ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            Draw.SpriteBatch.End();
            if (pauseMenu != null || pauseFade > 0f)
            {
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone, null, Engine.ScreenMatrix);
                Draw.Rect(-1f, -1f, 1922f, 1082f, Color.Black * pauseFade);
                if (pauseMenu != null)
                {
                    pauseMenu.Render();
                }

                Draw.SpriteBatch.End();
            }

            base.Render();
        }


        public void CreatePauseMenu()
        {
            Audio.Play("event:/ui/game/pause");
            Audio.PauseGameplaySfx = true;
            snapshot = Audio.CreateSnapshot("snapshot:/pause_menu");
            TextMenu menu = new TextMenu();
            menu.Add(new TextMenu.Button(Dialog.Clean("pico8_pause_continue")).Pressed(() =>
            {
                menu.OnCancel();
            }));
            menu.Add(new TextMenu.Button(Dialog.Clean("pico8_pause_restart")).Pressed(() =>
            {
                pauseMenu = null;
                music(-1, 0, 0);
                new FadeWipe(this, wipeIn: false, () =>
                {
                    Audio.BusStopAll("bus:/gameplay_sfx");
                    Audio.PauseGameplaySfx = false;
                    Audio.ReleaseSnapshot(snapshot);
                    snapshot = null;
                    ResetScreen();
                    game = null;
                    gameFrame = 0;
                    gameActive = true;
                    new FadeWipe(this, wipeIn: true);
                });
            }));
            menu.Add(new TextMenu.Button(Dialog.Clean("pico8_pause_quit")).Pressed(() =>
            {
                leaving = true;
                gameActive = false;
                pauseMenu = null;
                music(-1, 0, 0);
                new FadeWipe(this, wipeIn: false, () =>
                {
                    Audio.BusStopAll("bus:/gameplay_sfx");
                    Audio.PauseGameplaySfx = false;
                    Audio.ReleaseSnapshot(snapshot);
                    Audio.Stop(bgSfx);
                    snapshot = null;
                    if (ReturnTo != null)
                    {
                        if (ReturnTo is Level)
                        {
                            (ReturnTo as Level).Session.Audio.Apply(forceSixteenthNoteHack: false);
                            new FadeWipe(ReturnTo, wipeIn: true);
                        }

                        Engine.Scene = ReturnTo;
                    }
                    else
                    {
                        Engine.Scene = new OverworldLoader(Overworld.StartMode.Titlescreen);
                    }
                });
            }));
            menu.OnCancel = (menu.OnESC = (menu.OnPause = () =>
            {
                Audio.PauseGameplaySfx = false;
                Audio.ReleaseSnapshot(snapshot);
                snapshot = null;
                gameDelay = 0.1f;
                pauseMenu = null;
                gameActive = true;
                menu.RemoveSelf();
            }));
            gameActive = false;
            pauseMenu = menu;
        }


        public void music(int index, int fade, int mask)
        {
            switch (index)
            {
                case -1:
                    Audio.SetMusic(null);
                    break;
                case 0:
                    Audio.SetMusic("event:/classic/pico8_mus_01");
                    break;
                case 10:
                    Audio.SetMusic("event:/classic/pico8_mus_03");
                    break;
                case 20:
                    Audio.SetMusic("event:/classic/pico8_mus_02");
                    break;
                case 30:
                    Audio.SetMusic("event:/classic/sfx61");
                    break;
                case 40:
                    Audio.SetMusic("event:/classic/pico8_mus_00");
                    break;
            }
        }


        public void sfx(int sfx)
        {
            Audio.Play("event:/classic/sfx" + sfx);
        }

        public float rnd(float max)
        {
            return Calc.Random.NextFloat(max);
        }

        public int flr(float value)
        {
            return (int)Math.Floor(value);
        }

        public int sign(float value)
        {
            return Math.Sign(value);
        }

        public float abs(float value)
        {
            return Math.Abs(value);
        }

        public float min(float a, float b)
        {
            return Math.Min(a, b);
        }

        public float max(float a, float b)
        {
            return Math.Max(a, b);
        }


        public float sin(float a)
        {
            return (float)Math.Sin((1f - a) * (MathF.PI * 2f));
        }


        public float cos(float a)
        {
            return (float)Math.Cos((1f - a) * (MathF.PI * 2f));
        }

        public float mod(float a, float b)
        {
            float num = a % b;
            if (!(num >= 0f))
            {
                return b + num;
            }

            return num;
        }


        public bool btn(int index)
        {
            Vector2 vector = new Vector2(Input.MoveX, Input.MoveY);
            return index switch
            {
                0 => vector.X < 0f,
                1 => vector.X > 0f,
                2 => vector.Y < 0f,
                3 => vector.Y > 0f,
                4 => Input.Jump.Check,
                5 => Input.Dash.Check,
                _ => false,
            };
        }



        public Vector2 aimVector()
        {
            Vector2 vector = Input.Aim.Value;
            if (vector != Vector2.Zero)
            {
                if (SaveData.Instance != null && SaveData.Instance.Assists.ThreeSixtyDashing)
                {
                    vector = vector.SafeNormalize();
                }
                else
                {
                    float num = vector.Angle();
                    int num2 = ((num < 0f) ? 1 : 0);
                    float num3 = MathF.PI / 8f - (float)num2 * 0.08726646f;
                    vector = ((Calc.AbsAngleDiff(num, 0f) < num3) ? new Vector2(1f, 0f) : ((Calc.AbsAngleDiff(num, MathF.PI) < num3) ? new Vector2(-1f, 0f) : ((Calc.AbsAngleDiff(num, -MathF.PI / 2f) < num3) ? new Vector2(0f, -1f) : ((!(Calc.AbsAngleDiff(num, MathF.PI / 2f) < num3)) ? new Vector2(Math.Sign(vector.X), Math.Sign(vector.Y)).SafeNormalize() : new Vector2(0f, 1f)))));
                }
            }

            return vector;
        }

        public int dashDirectionX(int facing)
        {
            return Math.Sign(aimVector().X);
        }

        public int dashDirectionY(int facing)
        {
            return Math.Sign(aimVector().Y);
        }

        public int mget(int tx, int ty)
        {
            return tilemap[tx + ty * 128];
        }


        public bool fget(int tile, int flag)
        {
            if (tile < mask.Length)
            {
                return (mask[tile] & (1 << flag)) != 0;
            }

            return false;
        }

        public void camera()
        {
            offset = Vector2.Zero;
        }


        public void camera(float x, float y)
        {
            offset = new Vector2((int)Math.Round(x), (int)Math.Round(y));
        }

        public void pal()
        {
            paletteSwap.Clear();
        }


        public void pal(int a, int b)
        {
            Color key = colors[a];
            if (paletteSwap.ContainsKey(key))
            {
                paletteSwap[key] = b;
            }
            else
            {
                paletteSwap.Add(key, b);
            }
        }


        public void rectfill(float x, float y, float x2, float y2, float c)
        {
            float num = Math.Min(x, x2);
            float num2 = Math.Min(y, y2);
            float width = Math.Max(x, x2) - num + 1f;
            float height = Math.Max(y, y2) - num2 + 1f;
            Draw.Rect(num, num2, width, height, colors[(int)c % 16]);
        }


        public void circfill(float x, float y, float r, float c)
        {
            Color color = colors[(int)c % 16];
            if (r <= 1f)
            {
                Draw.Rect(x - 1f, y, 3f, 1f, color);
                Draw.Rect(x, y - 1f, 1f, 3f, color);
            }
            else if (r <= 2f)
            {
                Draw.Rect(x - 2f, y - 1f, 5f, 3f, color);
                Draw.Rect(x - 1f, y - 2f, 3f, 5f, color);
            }
            else if (r <= 3f)
            {
                Draw.Rect(x - 3f, y - 1f, 7f, 3f, color);
                Draw.Rect(x - 1f, y - 3f, 3f, 7f, color);
                Draw.Rect(x - 2f, y - 2f, 5f, 5f, color);
            }
        }


        public void print(string str, float x, float y, float c)
        {
            float num = x;
            Color color = colors[(int)c % 16];
            foreach (char c2 in str)
            {
                int num2 = -1;
                for (int j = 0; j < fontMap.Length; j++)
                {
                    if (fontMap[j] == c2)
                    {
                        num2 = j;
                        break;
                    }
                }

                if (num2 >= 0)
                {
                    font[num2].Draw(new Vector2(num, y), Vector2.Zero, color);
                }

                num += 4f;
            }
        }


        public void map(int mx, int my, int tx, int ty, int mw, int mh, int mask = 0)
        {
            for (int i = 0; i < mw; i++)
            {
                for (int j = 0; j < mh; j++)
                {
                    byte b = tilemap[i + mx + (j + my) * 128];
                    if (b < sprites.Length && (mask == 0 || fget(b, mask)))
                    {
                        sprites[b].Draw(new Vector2(tx + i * 8, ty + j * 8));
                    }
                }
            }
        }


        public void spr(float sprite, float x, float y, int columns = 1, int rows = 1, bool flipX = false, bool flipY = false)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (flipX)
            {
                spriteEffects |= SpriteEffects.FlipHorizontally;
            }

            if (flipY)
            {
                spriteEffects |= SpriteEffects.FlipVertically;
            }

            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    sprites[(int)sprite + i + j * 16].Draw(new Vector2((int)Math.Floor(x + (float)(i * 8)), (int)Math.Floor(y + (float)(j * 8))), Vector2.Zero, Color.White, 1f, 0f, spriteEffects);
                }
            }
        }



        public void orig_ctor(Scene returnTo, int levelX = 0, int levelY = 0)
        {
            gameActive = true;
            skipFrame = true;
            pixels = new Color[16384];
            offset = Vector2.Zero;
            mask = new byte[128]
            {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 4, 2, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 2, 0, 0,
            0, 0, 3, 3, 3, 3, 3, 3, 3, 3,
            4, 4, 4, 2, 2, 0, 0, 0, 3, 3,
            3, 3, 3, 3, 3, 3, 4, 4, 4, 2,
            2, 2, 2, 2, 0, 0, 19, 19, 19, 19,
            2, 2, 3, 2, 2, 2, 2, 2, 2, 2,
            0, 0, 19, 19, 19, 19, 2, 2, 4, 2,
            2, 2, 2, 2, 2, 2, 0, 0, 19, 19,
            19, 19, 0, 4, 4, 2, 2, 2, 2, 2,
            2, 2, 0, 0, 19, 19, 19, 19, 0, 0,
            0, 2, 2, 2, 2, 2, 2, 2
            };
            colors = new Color[16]
            {
            Calc.HexToColor("000000"),
            Calc.HexToColor("1d2b53"),
            Calc.HexToColor("7e2553"),
            Calc.HexToColor("008751"),
            Calc.HexToColor("ab5236"),
            Calc.HexToColor("5f574f"),
            Calc.HexToColor("c2c3c7"),
            Calc.HexToColor("fff1e8"),
            Calc.HexToColor("ff004d"),
            Calc.HexToColor("ffa300"),
            Calc.HexToColor("ffec27"),
            Calc.HexToColor("00e436"),
            Calc.HexToColor("29adff"),
            Calc.HexToColor("83769c"),
            Calc.HexToColor("ff77a8"),
            Calc.HexToColor("ffccaa")
            };
            paletteSwap = new Dictionary<Color, int>();
            fontMap = "abcdefghijklmnopqrstuvwxyz0123456789~!@#$%^&*()_+-=?:.";

            Tracker = new Tracker();
            Entities = new EntityList(this);
            TagLists = new TagLists();
            RendererList = new RendererList(this);
            actualDepthLookup = new Dictionary<int, double>();
            HelperEntity = new Entity();
            Entities.Add(HelperEntity);

            ReturnTo = returnTo;
            bootLevel = new Point(levelX, levelY);
            buffer = VirtualContent.CreateRenderTarget("pico-8", 128, 128);
            MTexture mTexture = GFX.Game["pico8/atlas"];
            sprites = new MTexture[mTexture.Width / 8 * (mTexture.Height / 8)];
            for (int i = 0; i < mTexture.Height / 8; i++)
            {
                for (int j = 0; j < mTexture.Width / 8; j++)
                {
                    sprites[j + i * (mTexture.Width / 8)] = mTexture.GetSubtexture(j * 8, i * 8, 8, 8);
                }
            }

            string input = "2331252548252532323232323300002425262425252631323232252628282824252525252525323328382828312525253232323233000000313232323232323232330000002432323233313232322525252525482525252525252526282824252548252525262828282824254825252526282828283132323225482525252525\r\n252331323232332900002829000000242526313232332828002824262a102824254825252526002a2828292810244825282828290000000028282900000000002810000000372829000000002a2831482525252525482525323232332828242525254825323338282a283132252548252628382828282a2a2831323232322525\r\n252523201028380000002a0000003d24252523201028292900282426003a382425252548253300002900002a0031252528382900003a676838280000000000003828393e003a2800000000000028002425253232323232332122222328282425252532332828282900002a283132252526282828282900002a28282838282448\r\n3232332828282900000000003f2020244825262828290000002a243300002a2425322525260000000000000000003125290000000021222328280000000000002a2828343536290000000000002839242526212223202123313232332828242548262b000000000000001c00003b242526282828000000000028282828282425\r\n2340283828293a2839000000343522252548262900000000000030000000002433003125333d3f00000000000000003100001c3a3a31252620283900000000000010282828290000000011113a2828313233242526103133202828282838242525262b000000000000000000003b2425262a2828670016002a28283828282425\r\n263a282828102829000000000000312525323300000000110000370000003e2400000037212223000000000000000000395868282828242628290000000000002a2828290000000000002123283828292828313233282829002a002a2828242525332b0c00000011110000000c3b314826112810000000006828282828282425\r\n252235353628280000000000003a282426003d003a3900270000000000002125001a000024252611111111000000002c28382828283831332800000017170000002a000000001111000024261028290028281b1b1b282800000000002a2125482628390000003b34362b000000002824252328283a67003a28282829002a3132\r\n25333828282900000000000000283824252320201029003039000000005824480000003a31323235353536675800003c282828281028212329000000000000000000000000003436003a2426282800003828390000002a29000000000031323226101000000000282839000000002a2425332828283800282828390000001700\r\n2600002a28000000003a283a2828282425252223283900372858390068283132000000282828282820202828283921222829002a28282426000000000000000000000000000020382828312523000000282828290000000000163a67682828003338280b00000010382800000b00003133282828282868282828280000001700\r\n330000002867580000281028283422252525482628286720282828382828212200003a283828102900002a28382824252a0000002838242600000017170000000000000000002728282a283133390000282900000000000000002a28282829002a2839000000002a282900000000000028282838282828282828290000000000\r\n0000003a2828383e3a2828283828242548252526002a282729002a28283432250000002a282828000000002810282425000000002a282426000000000000000000000000000037280000002a28283900280000003928390000000000282800000028290000002a2828000000000000002a282828281028282828675800000000\r\n0000002838282821232800002a28242532322526003a2830000000002a28282400000000002a281111111128282824480000003a28283133000000000000171700013f0000002029000000003828000028013a28281028580000003a28290000002a280c0000003a380c00000000000c00002a2828282828292828290000003a\r\n00013a2123282a313329001111112425002831263a3829300000000000002a310000000000002834222236292a0024253e013a3828292a00000000000000000035353536000020000000003d2a28671422222328282828283900582838283d00003a290000000028280000000000000000002a28282a29000058100012002a28\r\n22222225262900212311112122222525002a3837282900301111110000003a2800013f0000002a282426290000002425222222232900000000000000171700002a282039003a2000003a003435353535252525222222232828282810282821220b10000000000b28100000000b0000002c00002838000000002a283917000028\r\n2548252526111124252222252525482500012a2828673f242222230000003828222223000012002a24260000001224252525252600000000171700000000000000382028392827080028676820282828254825252525262a28282122222225253a28013d0000006828390000000000003c0168282800171717003a2800003a28\r\n25252525252222252525252525252525222222222222222525482667586828282548260000270000242600000021252525254826171700000000000000000000002a2028102830003a282828202828282525252548252600002a2425252548252821222300000028282800000000000022222223286700000000282839002838\r\n2532330000002432323232323232252525252628282828242532323232254825253232323232323225262828282448252525253300000000000000000000005225253232323233313232323233282900262829286700000000002828313232322525253233282800312525482525254825254826283828313232323232322548\r\n26282800000030402a282828282824252548262838282831333828290031322526280000163a28283133282838242525482526000000000000000000000000522526000016000000002a10282838390026281a3820393d000000002a3828282825252628282829003b2425323232323232323233282828282828102828203125\r\n3328390000003700002a3828002a2425252526282828282028292a0000002a313328111111282828000028002a312525252526000000000000000000000000522526000000001111000000292a28290026283a2820102011111121222328281025252628382800003b24262b002a2a38282828282829002a2800282838282831\r\n28281029000000000000282839002448252526282900282067000000000000003810212223283829003a1029002a242532323367000000000000000000004200252639000000212300000000002122222522222321222321222324482628282832323328282800003b31332b00000028102829000000000029002a2828282900\r\n2828280016000000162a2828280024252525262700002a2029000000000000002834252533292a0000002a00111124252223282800002c46472c00000042535325262800003a242600001600002425252525482631323331323324252620283822222328292867000028290000000000283800111100001200000028292a1600\r\n283828000000000000003a28290024254825263700000029000000000000003a293b2426283900000000003b212225252526382867003c56573c4243435363633233283900282426111111111124252525482526201b1b1b1b1b24252628282825252600002a28143a2900000000000028293b21230000170000112867000000\r\n2828286758000000586828380000313232323320000000000000000000272828003b2426290000000000003b312548252533282828392122222352535364000029002a28382831323535353522254825252525252300000000003132332810284825261111113435361111111100000000003b3133111111111127282900003b\r\n2828282810290000002a28286700002835353536111100000000000011302838003b3133000000000000002a28313225262a282810282425252662636400000000160028282829000000000031322525252525252667580000002000002a28282525323535352222222222353639000000003b34353535353536303800000017\r\n282900002a0000000000382a29003a282828283436200000000000002030282800002a29000011110000000028282831260029002a282448252523000000000039003a282900000000000000002831322525482526382900000017000058682832331028293b2448252526282828000000003b201b1b1b1b1b1b302800000017\r\n283a0000000000000000280000002828283810292a000000000000002a3710281111111111112136000000002a28380b2600000000212525252526001c0000002828281000000000001100002a382829252525252628000000001700002a212228282908003b242525482628282912000000001b00000000000030290000003b\r\n3829000000000000003a102900002838282828000000000000000000002a2828223535353535330000000000002828393300000000313225252533000000000028382829000000003b202b00682828003232323233290000000000000000312528280000003b3132322526382800170000000000000000110000370000000000\r\n290000000000000000002a000000282928292a0000000000000000000000282a332838282829000000000000001028280000000042434424252628390000000028002a0000110000001b002a2010292c1b1b1b1b0000000000000000000010312829160000001b1b1b313328106700000000001100003a2700001b0000000000\r\n00000100000011111100000000002a3a2a0000000000000000000000002a2800282829002a000000000000000028282800000000525354244826282800000000290000003b202b39000000002900003c000000000000000000000000000028282800000000000000001b1b2a2829000001000027390038300000000000000000\r\n1111201111112122230000001212002a00010000000000000000000000002900290000000000000000002a6768282900003f01005253542425262810673a3900013f0000002a3829001100000000002101000000000000003a67000000002a382867586800000100000000682800000021230037282928300000000000000000\r\n22222222222324482611111120201111002739000017170000001717000000000001000000001717000000282838393a0021222352535424253328282838290022232b00000828393b27000000001424230000001200000028290000000000282828102867001717171717282839000031333927101228370000000000000000\r\n254825252526242526212222222222223a303800000000000000000000000000001717000000000000003a28282828280024252652535424262828282828283925262b00003a28103b30000000212225260000002700003a28000000000000282838282828390000005868283828000022233830281728270000000000000000\r\n00000000000000008242525252528452339200001323232352232323232352230000000000000000b302000013232352526200a2828342525223232323232323\r\n00000000000000a20182920013232352363636462535353545550000005525355284525262b20000000000004252525262828282425284525252845252525252\r\n00000000000085868242845252525252b1006100b1b1b1b103b1b1b1b1b103b100000000000000111102000000a282425233000000a213233300009200008392\r\n000000000000110000a2000000a28213000000002636363646550000005525355252528462b2a300000000004252845262828382132323232323232352528452\r\n000000000000a201821323525284525200000000000000007300000000007300000000000000b343536300410000011362b2000000000000000000000000a200\r\n0000000000b302b2002100000000a282000000000000000000560000005526365252522333b28292001111024252525262019200829200000000a28213525252\r\n0000000000000000a2828242525252840000000000000000b10000000000b1000000000000000000b3435363930000b162273737373737373737374711000061\r\n000000110000b100b302b20000006182000000000000000000000000005600005252338282828201a31222225252525262820000a20011111100008283425252\r\n0000000000000093a382824252525252000061000011000000000011000000001100000000000000000000020182001152222222222222222222222232b20000\r\n0000b302b200000000b10000000000a200000000000000009300000000000000846282828283828282132323528452526292000000112434440000a282425284\r\n00000000000000a2828382428452525200000000b302b2936100b302b20061007293a30000000000000000b1a282931252845252525252232323232362b20000\r\n000000b10000001100000000000000000000000093000086820000a3000000005262828201a200a282829200132323236211111111243535450000b312525252\r\n00000000000000008282821323232323820000a300b1a382930000b100000000738283931100000000000011a382821323232323528462829200a20173b20061\r\n000000000000b302b2000061000000000000a385828286828282828293000000526283829200000000a20000000000005222222232263636460000b342525252\r\n00000011111111a3828201b1b1b1b1b182938282930082820000000000000000b100a282721100000000b372828283b122222232132333610000869200000000\r\n00100000000000b1000000000000000086938282828201920000a20182a37686526282829300000000000000000000005252845252328283920000b342845252\r\n00008612222232828382829300000000828282828283829200000000000061001100a382737200000000b373a2829211525284628382a2000000a20000000000\r\n00021111111111111111111111110061828282a28382820000000000828282825262829200000000000000000000000052525252526201a2000000b342525252\r\n00000113235252225353536300000000828300a282828201939300001100000072828292b1039300000000b100a282125223526292000000000000a300000000\r\n0043535353535353535353535363b2008282920082829200061600a3828382a28462000000000000000000000000000052845252526292000011111142525252\r\n0000a28282132362b1b1b1b1000000009200000000a28282828293b372b2000073820100110382a3000000110082821362101333610000000000008293000000\r\n0002828382828202828282828272b20083820000a282d3000717f38282920000526200000000000093000000000000005252525284620000b312223213528452\r\n000000828392b30300000000002100000000000000000082828282b303b20000b1a282837203820193000072a38292b162710000000000009300008382000000\r\n00b1a282820182b1a28283a28273b200828293000082122232122232820000a3233300000000000082920000000000002323232323330000b342525232135252\r\n000000a28200b37300000000a37200000010000000111111118283b373b200a30000828273039200828300738283001162930000000000008200008282920000\r\n0000009261a28200008261008282000001920000000213233342846282243434000000000000000082000085860000008382829200000000b342528452321323\r\n0000100082000082000000a2820300002222321111125353630182829200008300009200b1030000a28200008282001262829200000000a38292008282000000\r\n00858600008282a3828293008292610082001000001222222252525232253535000000f3100000a3820000a2010000008292000000009300b342525252522222\r\n0400122232b200839321008683039300528452222262c000a28282820000a38210000000a3738000008293008292001362820000000000828300a38201000000\r\n00a282828292a2828283828282000000343434344442528452525252622535350000001263000083829300008200c1008210d3e300a38200b342525252845252\r\n1232425262b28682827282820103820052525252846200000082829200008282320000008382930000a28201820000b162839300000000828200828282930000\r\n0000008382000000a28201820000000035353535454252525252528462253535000000032444008282820000829300002222223201828393b342525252525252\r\n525252525262b2b1b1b1132323526200845223232323232352522323233382825252525252525252525284522333b2822323232323526282820000b342525252\r\n52845252525252848452525262838242528452522333828292425223232352520000000000000000000000000000000000000000000000000000000000000000\r\n525252845262b2000000b1b1b142620023338276000000824233b2a282018283525252845252232323235262b1b10083921000a382426283920000b342232323\r\n2323232323232323232323526201821352522333b1b1018241133383828242840000000000000000000000000000000000000000000000000000000000000000\r\n525252525262b20000000000a242627682828392000011a273b200a382729200525252525233b1b1b1b11333000000825353536382426282410000b30382a2a2\r\na1829200a2828382820182426200a2835262b1b10000831232b2000080014252000000000000a300000000000000000000000000000000000000000000000000\r\n528452232333b20000001100824262928201a20000b3720092000000830300002323525262b200000000b3720000a382828283828242522232b200b373928000\r\n000100110092a2829211a2133300a3825262b2000000a21333b20000868242520000000000000100009300000000000000000000000000000000000000000000\r\n525262122232b200a37672b2a24262838292000000b30300000000a3820300002232132333b200000000b303829300a2838292019242845262b2000000000000\r\n00a2b302b2a36182b302b200110000825262b200000000b1b10000a283a2425200000000a30082000083000000000000000000000094a4b4c4d4e4f400000000\r\n525262428462b200a28303b2214262928300000000b3030000000000a203e3415252222232b200000000b30392000000829200000042525262b2000000000000\r\n000000b100a2828200b100b302b211a25262b200000000000000000092b3428400000000827682000001009300000000000000000095a5b5c5d5e5f500000000\r\n232333132362b221008203b2711333008293858693b3031111111111114222225252845262b200001100b303b2000000821111111142528462b2000000000000\r\n000000000000110176851100b1b3026184621111111100000061000000b3135200000000828382670082768200000000000000000096a6b6c6d6e6f600000000\r\n82000000a203117200a203b200010193828283824353235353535353535252845252525262b200b37200b303b2000000824353535323235262b2000011000000\r\n0000000000b30282828372b26100b100525232122232b200000000000000b14200000000a28282123282839200000000000000000097a7b7c7d7e7f700000000\r\n9200110000135362b2001353535353539200a2000001828282829200b34252522323232362b261b30300b3030000000092b1b1b1b1b1b34262b200b372b20000\r\n001100000000b1a2828273b200000000232333132333b200001111000000b342000000868382125252328293a300000000000000000000000000000000000000\r\n00b372b200a28303b2000000a28293b3000000000000a2828382827612525252b1b1b1b173b200b30393b30361000000000000000000b34262b271b303b20000\r\nb302b211000000110092b100000000a3b1b1b1b1b1b10011111232110000b342000000a282125284525232828386000000000000000000000000000000000000\r\n80b303b20000820311111111008283b311111111110000829200928242528452000000a3820000b30382b37300000000000000000000b3426211111103b20000\r\n00b1b302b200b372b200000000000082b21000000000b31222522363b200b3138585868292425252525262018282860000000000000000000000000000000000\r\n00b373b20000a21353535363008292b32222222232111102b20000a21323525200000001839200b3038282820000000011111111930011425222222233b20000\r\n100000b10000b303b200000000858682b27100000000b3425233b1b1000000b182018283001323525284629200a2820000000000000000000000000000000000\r\n9300b100000000b1b1b1b1b100a200b323232323235363b100000000b1b1135200000000820000b30382839200000000222222328283432323232333b2000000\r\n329300000000b373b200000000a20182111111110000b31333b100a30061000000a28293f3123242522333020000820000000000000000000000000000000000\r\n829200001000410000000000000000b39310d30000a28200000000000000824200000086827600b30300a282760000005252526200828200a30182a2006100a3\r\n62820000000000b100000093a382838222222232b20000b1b1000083000000860000122222526213331222328293827600000000000000000000000000000000\r\n017685a31222321111111111002100b322223293000182930000000080a301131000a383829200b373000083920000005284526200a282828283920000000082\r\n62839321000000000000a3828282820152845262b261000093000082a300a3821000135252845222225252523201838200000000000000000000000000000000\r\n828382824252522222222232007100b352526282a38283820000000000838282320001828200000083000082010000005252526271718283820000000000a382\r\n628201729300000000a282828382828252528462b20000a38300a382018283821222324252525252525284525222223200000000000000000000000000000000";
            input = Regex.Replace(input, "\\s+", "");
            tilemap = new byte[input.Length / 2];
            int k = 0;
            int length = input.Length;
            int num = length / 2;
            for (; k < length; k += 2)
            {
                char c = input[k];
                char c2 = input[k + 1];
                string s = ((k < num) ? (c.ToString() + c2) : (c2.ToString() + c));
                tilemap[k / 2] = (byte)int.Parse(s, NumberStyles.HexNumber);
            }

            MTexture mTexture2 = GFX.Game["pico8/font"];
            font = new MTexture[mTexture2.Width / 4 * (mTexture2.Height / 6)];
            for (int l = 0; l < mTexture2.Height / 6; l++)
            {
                for (int m = 0; m < mTexture2.Width / 4; m++)
                {
                    font[m + l * (mTexture2.Width / 4)] = mTexture2.GetSubtexture(m * 4, l * 6, 4, 6);
                }
            }

            picoBootLogo = GFX.Game["pico8/logo"];
            ResetScreen();
            Audio.SetMusic(null);
            Audio.SetAmbience(null);
            new FadeWipe(this, wipeIn: true);
            base.RendererList.UpdateLists();
        }
    }
}
