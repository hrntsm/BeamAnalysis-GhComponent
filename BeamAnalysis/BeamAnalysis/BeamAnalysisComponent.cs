using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Attributes;
using Grasshopper.GUI;
using Grasshopper.GUI.HTML;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI.Gradient;

using GH_IO.Serialization;

using Rhino.Geometry;

namespace BeamAnalysis
{
    public class BeamAnalysisComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public BeamAnalysisComponent()
          : base("BeamAnalysis",                     // 名称
                 "BeamAnalysis",                     // 略称
                 "Stress Analysis of the Beam",      // コンポーネントの説明
                 "rgkr",                             // カテゴリ(タブの表示名)
                 "Beam ANLYS"                        // サブカテゴリ(タブ内の表示名)
                )
        {
        }
        
        /// <summary>
        /// UIのカスタム
        /// </summary>
        public override void CreateAttributes()
        {
            m_attributes = new UI_Setting.Attributes_Custom(this);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Analysis Parametar", "Param", "Input Analysis Parameter", GH_ParamAccess.list);
            pManager.AddNumberParameter("Load", "Load", "Centralized load (kN)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Young's modulus", "E", "Young's modulus (N/mm^2)", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Bending Moment", "M", "output max bending moment(kNm)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Bending Stress", "Sig", "output max bending stress (N/mm^2)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Deformation", "D", "output max deformation(mm)", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // input
            // パラメータはひとまとめにするため、List にまとめる
            List<double> Param = new List<double>();
            double P = double.NaN;
            double E = double.NaN;
            // output
            double M = double.NaN;
            double Sig = double.NaN;
            double D = double.NaN;
            //
            double L, Iy, Zy;

            // Paramは List なので、GetDataList とする。
            if (!DA.GetDataList(0, Param)) { return; }
            if (!DA.GetData(1, ref P)) { return; }
            if (!DA.GetData(2, ref E)) { return; }

            L = Param[0];
            Iy = Param[1];
            Zy = Param[2];

            M = P * (L / 1000) / 4;
            Sig = M * 1000000 / Zy;
            D = P * 1000 * L * L * L / (48 * E * Iy);
            
            // 出力設定
            DA.SetData(0, M);
            DA.SetData(1, Sig);
            DA.SetData(2, D);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // アイコンの設定（未設定）
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// GUIDの設定
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("621eac03-23fb-445c-9430-44ce37bf9020"); }
        }

        /// <summary>
        /// コンポーネントを右クリック時に出るコンテキストメニューの編集
        /// </summary>
        public override bool AppendMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendObjectName(menu);          // オブジェクト名
            Menu_AppendSeparator(menu);           // セパレータ
            Menu_AppendPreviewItem(menu);         // プレビュー
            Menu_AppendEnableItem(menu);          // Enable (コンポーネントの有効化)
            Menu_AppendBakeItem(menu);            // ベーク
            Menu_AppendSeparator(menu);           // セパレータ
            Menu_AppendItem(menu, "Buckling Consideration",        // 追加部分
                            Menu_MyCustomItemClicked);
            Menu_AppendSeparator(menu);           // セパレータ
            Menu_AppendItem(menu, "test");       // 追加部分
            Menu_AppendSeparator(menu);           // セパレータ
            Menu_AppendRuntimeMessages(menu);     // ランタイムメッセージ
            Menu_AppendSeparator(menu);           // セパレータ
            Menu_AppendObjectHelp(menu);          // ヘルプ
            return true;
        }

        /// <summary>
        /// 座屈考慮のフラグの処理
        /// このままだとずっとfalseのままなので要修正
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu_MyCustomItemClicked(Object sender, EventArgs e)
        {
            bool BucklingConsideration = false;

            if (BucklingConsideration == true)
            {
                BucklingConsideration = true;
            }
            else {
                BucklingConsideration = false;
            }
            string test = BucklingConsideration.ToString();
            Rhino.RhinoApp.WriteLine("BucklingConsideration:"+ test);
        }
    }
}

/// <summary>
/// rhino上への出力関連の設定
/// </summary>
namespace ModelDisp
{
    public class H_Shape_Model : GH_Component
    {
        public H_Shape_Model() : base("Make H Shape Model", "H Steel", "Display H Shape Model", "rgkr", "H-Shape")
        {
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Width", "B", "Model Width (mm)", GH_ParamAccess.item, 200.0 );
            pManager.AddNumberParameter("Height", "H", "Model High (mm)", GH_ParamAccess.item, 400.0);
            pManager.AddNumberParameter("Web Thickness", "tw", "Web Thickness (mm)", GH_ParamAccess.item, 8.0);
            pManager.AddNumberParameter("Flange Thickness", "tf", "Flange Thickness (mm)", GH_ParamAccess.item, 13.0);
            pManager.AddNumberParameter("Length", "L", "Model Length (mm)", GH_ParamAccess.item, 3000.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Analysis Parametar", "Param", "output Analysis Parameter", GH_ParamAccess.item);
            pManager.AddSurfaceParameter("View Model Surface", "Srf", "output Model Surface", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 引数設定
            double B = double.NaN;
            double H = double.NaN;
            double L = double.NaN;
            double tw = double.NaN;
            double tf = double.NaN;
            double Iy, Zy;

            double def_B = 300.0;
            double def_H = 500.0;
            double def_tw = 9.0;
            double def_tf = 16.0;
            double def_L = 3000.0;

            // 入力設定
            if (!DA.GetData(0, ref B))  { return; }
            if (!DA.GetData(1, ref H))  { return; }
            if (!DA.GetData(2, ref tw)) { return; }
            if (!DA.GetData(3, ref tf)) { return; }
            if (!DA.GetData(4, ref L))  { return; }

            if (Double.IsNaN(B))
            {
                B = def_B;
            }

            // 原点の作成
            var Ori = new Point3d(0, 0, 0);

            // 上フランジのサーフェス作成
            var UFp1 = new Point3d(0, 0, H / 2);
            var UFp2 = new Point3d(1, 0, H / 2);
            var UFp3 = new Point3d(0, 1, H / 2);
            var UFplane = new Plane(UFp1, UFp2, UFp3);
            var upper_flange = new PlaneSurface(UFplane, new Interval(-B / 2, B / 2), new Interval(0, L));

            // 下フランジのサーフェス作成
            var BFp1 = new Point3d(0, 0, -H / 2);
            var BFp2 = new Point3d(1, 0, -H / 2);
            var BFp3 = new Point3d(0, 1, -H / 2);
            var BFplane = new Plane(BFp1, BFp2, BFp3);
            var bottom_flange = new PlaneSurface(BFplane, new Interval(-B / 2, B / 2), new Interval(0, L));

            // ウェブのサーフェス作成
            var Wp1 = new Point3d(0, 0, 0);
            var Wp2 = new Point3d(0, 0, -1);
            var Wp3 = new Point3d(0, 1, 0);
            var Wplane = new Plane(Wp1, Wp2, Wp3);
            var web = new PlaneSurface(Wplane, new Interval(-H / 2, H / 2), new Interval(0, L));

            // 解析用パラメータの計算
            Iy = (1.0 / 12.0 * B * H * H * H) - (1.0 / 12.0 * (B - tw) * (H - 2 * tf) * (H - 2 * tf) * (H - 2 * tf));
            Zy = Iy / (H / 2);
            // ひとまとめにするため List で作成
            List<double> Params = new List<double>();
            Params.Add(L);  //  部材長さ
            Params.Add(Iy); //  断面二次モーメント
            Params.Add(Zy); //  断面係数

            // モデルはRhino上に出力するだけなので、とりあえず配列でまとめる
            var model = new PlaneSurface[3];
            model[0] = upper_flange;
            model[1] = bottom_flange;
            model[2] = web;

            // まとめての出力なので、SetDataList で出力
            DA.SetDataList(0, model);
            DA.SetDataList(1, Params);
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("419c3a3a-cc48-4717-8cef-5f5647a5ecAa"); }
        }
    }
}

/// <summary>
/// UI にかかわる設定
/// </summary>
namespace UI_Setting
{
    /// <summary>
    /// ボタンを押すとWindowsFormを出力させる
    /// </summary>
    public class Attributes_Custom : GH_ComponentAttributes
    {
        public Attributes_Custom(GH_Component owner) : base(owner)
        {
        }
        /// <summary>
        /// ボタンの箱(Rectangle)をを設定する。
        /// サイズが直接指定なので、汎用性は低め
        /// 今後は引数から箱のサイズを決めれるようにしたい。
        /// </summary>
        protected override void Layout()
        {
            base.Layout();

            Rectangle rec0 = GH_Convert.ToRectangle(Bounds);
            rec0.Height += 22;

            Rectangle rec1 = rec0;
            rec1.Y = rec1.Bottom - 22;
            rec1.Height = 22;
            rec1.Inflate(-2, -2);
            
            Bounds = rec0;
            ButtonBounds = rec1;

        }

        private Rectangle ButtonBounds
        {
            get;
            set;
        }

        /// <summary>
        /// ラジオボタンの作りかけ
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="center"></param>
        /// <param name="Checked"></param>
        private void DrawRadioButton(Graphics graphics, PointF center, bool Checked )
        {
            if (Checked)
            {
                graphics.FillEllipse(Brushes.Black, center.X - 6, center.Y - 6, 12, 12);
            }
            else
            {
                graphics.FillEllipse(Brushes.Black, center.X - 6, center.Y - 6, 12, 12);
                graphics.FillEllipse(Brushes.White, center.X - 4, center.Y - 4,  8,  8);
            }
        }

        /// <summary>
        /// 入力された箱からgrasshopperで認識されるTextCapsuleを作成する。
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="graphics"></param>
        /// <param name="channel"></param>
        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);
            if (channel == GH_CanvasChannel.Objects)
            {
                GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds, ButtonBounds, GH_Palette.Black, "output", 2, 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                button.Dispose();
            }
        }

        /// <summary>
        /// マウスダウンした時のイベントハンドラ
        /// 左クリックした際にWindouwsFormを使用してメッセージボックスを出力させる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.Left)
            {
                RectangleF rec = ButtonBounds;
                if (rec.Contains(e.CanvasLocation))
                {
                    MessageBox.Show("入力データと解析結果を\nWindowsFormを使用して出力させる。", "結果のアウトプット", MessageBoxButtons.OK);
                    return GH_ObjectResponse.Handled;
                }
            }
            
            return base.RespondToMouseDown(sender, e);
        }
    }
}

