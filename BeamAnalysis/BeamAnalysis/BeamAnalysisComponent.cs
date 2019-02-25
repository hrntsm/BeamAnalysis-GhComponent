/**
Copyright (c) 2018 hiron_rgrk

This software is released under the MIT License.
See LICENSE
**/

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

using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;


namespace BeamAnalysis {
    public class Beam_CL_Analysis : GH_Component {
        /// <summary>
        /// 中央集中荷重の梁の計算
        /// </summary>
        public Beam_CL_Analysis()
          : base("Centralized Load",                 // 名称
                 "Centralized L",                    // 略称
                 "Stress Analysis of the Beam",      // コンポーネントの説明
                 "rgkr",                             // カテゴリ(タブの表示名)
                 "Beam Analysis"                     // サブカテゴリ(タブ内の表示名)
                ) {
        }
        public override void ClearData() {
            base.ClearData();
        }
        /// <summary>
        /// UIのカスタム 作り切れてないのでコメントアウト
        /// </summary>
        //      public override void CreateAttributes()
        //      {
        //          m_attributes = new UI_Setting.Attributes_Custom(this);
        //      }

        /// <summary>
        /// インプットパラメータの登録
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddNumberParameter("Analysis Parametar", "Param", "Input Analysis Parameter", GH_ParamAccess.list);
            pManager.AddNumberParameter("Load", "Load", "Centralized Load (kN)", GH_ParamAccess.item,100);
            pManager.AddNumberParameter("Lb", "Lb", "Buckling Length (mm)", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Young's modulus", "E", "Young's Modulus (N/mm^2)", GH_ParamAccess.item, 205000);
        }
        /// <summary>
        /// アウトプットパラメータの登録
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddNumberParameter("Bending Moment", "M", "Output Max Bending Moment(kNm)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Bending Stress", "Sig", "Output Max Bending Stress(N/mm^2)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Allowable Bending Stress", "fb", "Output Allowable Bending Stress(N/mm^2)", GH_ParamAccess.item);
            pManager.AddNumberParameter("examination result", "Sig/fb", "Output Max Examination Result", GH_ParamAccess.item);
            pManager.AddNumberParameter("Deformation", "D", "Output Max Deformation(mm)", GH_ParamAccess.item);
        }
        /// <summary>
        /// 計算部分
        /// </summary>
        /// <param name="DA">インプットパラメータからデータを取得し、出力用のデータを保持するオブジェクト</param>
        protected override void SolveInstance(IGH_DataAccess DA) {
            // 引数の宣言＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            // input
            List<double> Param = new List<double>();
            List<double> M_out = new List<double>();
            double P = double.NaN;
            double Lb = double.NaN;
            double E = double.NaN;
            // output
            double M = double.NaN;
            double Sig = double.NaN;
            double D = double.NaN;
            //
            double L, Iy, Zy;
            double C = 1.0;
            double fb = 0.0;

            // 入力設定＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            if (!DA.GetDataList(0, Param)) { return; }
            if (!DA.GetData(1, ref P)) { return; }
            if (!DA.GetData(2, ref Lb)) { return; }
            if (!DA.GetData(3, ref E)) { return; }

            // 必要な引数の割り当て＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            L = Param[1];
            Iy = Param[3];
            Zy = Param[4];

            // 梁の計算箇所＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            M = P * (L / 1000) / 4;
            Sig = M * 1000000 / Zy;
            D = P * 1000 * L * L * L / (48 * E * Iy);

            // モーメントの出力＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            M_out.Add(0);
            M_out.Add(M / 2);
            M_out.Add(M);
            M_out.Add(M / 2);
            M_out.Add(0);
            M_out.Add(L);

            // 許容曲げの計算＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            Solver.Calc_fb_solver slv = new Solver.Calc_fb_solver();
            slv.Calc_fb(Param, Lb, C, ref fb);

            // 出力設定＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            DA.SetDataList(0, M_out);
            DA.SetData(1, Sig);
            DA.SetData(2, fb);
            DA.SetData(3, Sig/fb);
            DA.SetData(4, D);
        }
        /// <summary>
        /// アイコンの設定。24x24 pixelsが推奨
        /// </summary>
        protected override System.Drawing.Bitmap Icon {
            get {
                return BeamAnalysis.Properties.Resource.CL_icon;
            }
        }
        /// <summary>
        /// GUIDの設定
        /// </summary>
        public override Guid ComponentGuid {
            get {
                return new Guid("621eac03-23fb-445c-9430-44ce37bf9020");
            }
        }
        /// <summary>
        /// コンポーネントを右クリック時に出るコンテキストメニューの編集
        /// 未完なのでコメントアウト
        /// </summary>
        //      public override bool AppendMenuItems(ToolStripDropDown menu)
        //      {
        //          Menu_AppendObjectName(menu);          // オブジェクト名
        //          Menu_AppendSeparator(menu);           // セパレータ
        //          Menu_AppendPreviewItem(menu);         // プレビュー
        //          Menu_AppendEnableItem(menu);          // Enable (コンポーネントの有効化)
        //          Menu_AppendBakeItem(menu);            // ベーク
        //          Menu_AppendSeparator(menu);           // セパレータ
        //          Menu_AppendItem(menu, "Buckling Consideration",        // 追加部分
        //                          Menu_MyCustomItemClicked);
        //          Menu_AppendSeparator(menu);           // セパレータ
        //          Menu_AppendItem(menu, "test");       // 追加部分
        //          Menu_AppendSeparator(menu);           // セパレータ
        //          Menu_AppendRuntimeMessages(menu);     // ランタイムメッセージ
        //          Menu_AppendSeparator(menu);           // セパレータ
        //          Menu_AppendObjectHelp(menu);          // ヘルプ
        //          return true;
        //      }

        /// <summary>
        /// 座屈考慮のフラグの処理
        /// このままだとずっとfalseのままなので要修正
        /// 未完なのでコメントアウト
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //      private void Menu_MyCustomItemClicked(Object sender, EventArgs e)
        //      {
        //          bool BucklingConsideration = false;
        //
        //          if (BucklingConsideration == true)
        //          {
        //              BucklingConsideration = true;
        //          }
        //          else {
        //              BucklingConsideration = false;
        //          }
        //          string test = BucklingConsideration.ToString();
        //          Rhino.RhinoApp.WriteLine("BucklingConsideration:"+ test);
        //      }

    }

    public class Beam_TL_Analysis : GH_Component {
        /// <summary>
        /// 台形分布荷重の梁の計算
        /// </summary>
        public Beam_TL_Analysis()
        : base("Trapezoid Load",                   // 名称
                "Trapezoid L",                      // 略称
                "Stress Analysis of the Beam",      // コンポーネントの説明
                "rgkr",                             // カテゴリ(タブの表示名)
                "Beam Analysis"                     // サブカテゴリ(タブ内の表示名)
            　) {
        }
        public override void ClearData() {
            base.ClearData();
        }
        /// <summary>
        /// インプットパラメータの登録
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddNumberParameter("Analysis Parametar", "Param", "Input Analysis Parameter", GH_ParamAccess.list);
            pManager.AddNumberParameter("Trapezoid Load", "W", "Trapezoid Load (kN/m^2)", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("D Width", "DW", "Domination Width (mm)", GH_ParamAccess.item, 1800);
            pManager.AddNumberParameter("Lb", "Lb", "Buckling Length (mm)", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Young's Modulus", "E", "Young's Modulus (N/mm^2)", GH_ParamAccess.item, 205000);
        }
        /// <summary>
        /// アウトプットパラメータの登録
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddNumberParameter("Bending Moment", "M", "Output Max Bending Moment (kNm)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Bending Stress", "Sig", "Output Max Bending Stress (N/mm^2)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Allowable Bending Stress", "fb", "Output Allowable Bending Stress (N/mm^2)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Examination Result", "Sig/fb", "Output Max Examination Result", GH_ParamAccess.item);
            pManager.AddNumberParameter("Deformation", "D", "Output Max Deformation(mm)", GH_ParamAccess.item);
        }
        /// <summary>
        /// 計算部分
        /// </summary>
        /// <param name="DA">インプットパラメータからデータを取得し、出力用のデータを保持するオブジェクト</param>
        protected override void SolveInstance(IGH_DataAccess DA) {
            // 引数の宣言＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            // input
            List<double> Param = new List<double>();
            List<double> M_out = new List<double>();
            double W = double.NaN;
            double DW = double.NaN;
            double Lb = double.NaN;
            double E = double.NaN;
            // output
            double M = double.NaN;
            double Sig = double.NaN;
            double D = double.NaN;
            //
            double L, Iy, Zy, Ra, Mx;
            double C = 1.0;
            double fb = 0.0;
            
            // 入力設定＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            if (!DA.GetDataList(0, Param)) { return; }
            if (!DA.GetData(1, ref W)) { return; }
            if (!DA.GetData(2, ref DW)) { return; }
            if (!DA.GetData(3, ref Lb)) { return; }
            if (!DA.GetData(4, ref E)) { return; }
        
            // 必要な引数の割り当て＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            L = Param[1];
            Iy = Param[3];
            Zy = Param[4];

            // 梁の計算箇所＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            M = ((W * (DW / 1000)) / 24 * (3 * L * L - 4 * DW * DW))/1000000;
            Sig = M * 1000000 / Zy;
            D = (W*(DW/1000)) / (1920 * E * Iy) * (5 * L * L - 4 * DW * DW) * (5 * L * L - 4 * DW * DW);
            Ra = (W* (DW / 1000)) * (L - DW) / 2; // 反力
            Mx = ((Ra * L / 4) - ((W * (DW / 1000)) * Math.Pow(L / 4, 3) / (6 * DW)))/1000000; // 1/4点のモーメント計算

            // モーメントの出力＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            M_out.Add(0);
            M_out.Add(Mx);
            M_out.Add(M);
            M_out.Add(Mx);
            M_out.Add(0);
            M_out.Add(L);
     
            // 許容曲げの計算 ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            Solver.Calc_fb_solver slv = new Solver.Calc_fb_solver();
            slv.Calc_fb( Param, Lb, C, ref fb);

            // 出力設定＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            DA.SetDataList(0, M_out);
            DA.SetData(1, Sig);
            DA.SetData(2, fb);
            DA.SetData(3, Sig / fb);
            DA.SetData(4, D);
        }
        /// <summary>
        /// アイコンの設定。24x24 pixelsが推奨
        /// </summary>
        protected override System.Drawing.Bitmap Icon {
            get {
                return BeamAnalysis.Properties.Resource.UL_icon;
            }
        }
        /// <summary>
        /// GUIDの設定
        /// </summary>
        public override Guid ComponentGuid {
            get {
                return new Guid("621eac11-23fb-445c-9430-44ce37bf9020");
            }
        }
    }
    public class Beam_Canti_Analysis : GH_Component {
        /// <summary>
        /// 先端集中荷重の片持ち梁の計算
        /// </summary>
        public Beam_Canti_Analysis()
         : base("Cantilever Point Load",            // 名称
                "Canti PL",                         // 略称
                "Stress Analysis of the Beam",      // コンポーネントの説明
                "rgkr",                             // カテゴリ(タブの表示名)
                "Beam Analysis"                     // サブカテゴリ(タブ内の表示名)
            　) {
        }
        public override void ClearData() {
            base.ClearData();
        }
        /// <summary>
        /// インプットパラメータの登録
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddNumberParameter("Analysis Parametar", "Param", "Input Analysis Parameter", GH_ParamAccess.list);
            pManager.AddNumberParameter("Point Load", "P", "Point Load (kN)", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("Lb", "Lb", "Buckling Length (mm)", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Young's Modulus", "E", "Young's Modulus (N/mm^2)", GH_ParamAccess.item, 205000);
        }
        /// <summary>
        /// アウトプットパラメータの登録
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddNumberParameter("Bending Moment", "M", "Output Max Bending Moment (kNm)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Bending Stress", "Sig", "Output Max Bending Stress (N/mm^2)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Allowable Bending Stress", "fb", "Output Allowable Bending Stress (N/mm^2)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Examination Result", "Sig/fb", "Output Max Examination Result", GH_ParamAccess.item);
            pManager.AddNumberParameter("Deformation", "D", "Output Max Deformation(mm)", GH_ParamAccess.item);
        }
        /// <summary>
        /// 計算部分
        /// </summary>
        /// <param name="DA">インプットパラメータからデータを取得し、出力用のデータを保持するオブジェクト</param>
        protected override void SolveInstance(IGH_DataAccess DA) {
            // 引数の宣言＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            // input
            List<double> Param = new List<double>();
            List<double> M_out = new List<double>();
            double P = double.NaN;
            double Lb = double.NaN;
            double E = double.NaN;
            // output
            double M = double.NaN;
            double Sig = double.NaN;
            double D = double.NaN;
            //
            double L, Iy, Zy;
            double C = 1.0;
            double fb = 0.0;

            // 入力設定＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            if (!DA.GetDataList(0, Param)) { return; }
            if (!DA.GetData(1, ref P)) { return; }
            if (!DA.GetData(2, ref Lb)) { return; }
            if (!DA.GetData(3, ref E)) { return; }

            // 必要な引数の割り当て＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            L = Param[1];
            Iy = Param[3];
            Zy = Param[4];

            // 梁の計算箇所＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            M =  P * L/ 1000;
            Sig = M * 1000000 / Zy;
            D = (P /1000) * L * L * L / (3.0 * E * Iy); 

            // モーメントの出力＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            M_out.Add(-M);
            M_out.Add(-M * 3/4);
            M_out.Add(-M * 1/2);
            M_out.Add(-M * 1/4);
            M_out.Add(0);
            M_out.Add(L);

            // 許容曲げの計算 ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            Solver.Calc_fb_solver slv = new Solver.Calc_fb_solver();
            slv.Calc_fb(Param, Lb, C, ref fb);

            // 出力設定＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            DA.SetDataList(0, M_out);
            DA.SetData(1, Sig);
            DA.SetData(2, fb);
            DA.SetData(3, Sig / fb);
            DA.SetData(4, D);
        }
        /// <summary>
        /// アイコンの設定。24x24 pixelsが推奨
        /// </summary>
        protected override System.Drawing.Bitmap Icon {
            get {
                return BeamAnalysis.Properties.Resource.CantiPL_icon;
            }
        }
        /// <summary>
        /// GUIDの設定
        /// </summary>
        public override Guid ComponentGuid {
            get {
                return new Guid("621eac11-23fb-445c-9430-44ce37bf9031");
            }
        }
    }
    public class Beam_AnyM_Analysis : GH_Component {
        /// <summary>
        /// 任意荷重のかかった梁の計算
        /// </summary>
        public Beam_AnyM_Analysis()
      : base("Any Moment",     　　　　　        // 名称
             "Any M",                            // 略称
             "Stress Analysis of the Beam",      // コンポーネントの説明
             "rgkr",                             // カテゴリ(タブの表示名)
             "Beam Analysis"                     // サブカテゴリ(タブ内の表示名)
            ) {
        }
        public override void ClearData() {
            base.ClearData();
        }
        /// <summary>
        /// インプットパラメータの登録
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddNumberParameter("Analysis Parametar", "Param", "Input Analysis Parameter", GH_ParamAccess.list);
            pManager.AddNumberParameter("Any Moment", "AnyM", "Any Moment (kNm)", GH_ParamAccess.item, 1000);
            pManager.AddNumberParameter("Lb", "Lb", "Buckling Length (mm)", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Young's Modulus", "E", "Young's Modulus (N/mm^2)", GH_ParamAccess.item, 205000);
        }
        /// <summary>
        /// アウトプットパラメータの登録
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddNumberParameter("Bending Moment", "M", "Output Max Bending Moment(kNm)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Bending Stress", "Sig", "Output Max Bending Stress(N/mm^2)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Allowable Bending stress", "fb", "Output Allowable Bending Stress(N/mm^2)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Examination Result", "Sig/fb", "Output Max Examination Result", GH_ParamAccess.item);
            pManager.AddNumberParameter("Deformation", "D", "Output Max Deformation(mm)", GH_ParamAccess.item);
        }
        /// <summary>
        /// 計算部分
        /// </summary>
        /// <param name="DA">インプットパラメータからデータを取得し、出力用のデータを保持するオブジェクト</param>
        protected override void SolveInstance(IGH_DataAccess DA) {
            // 引数の宣言＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            // input
            List<double> Param = new List<double>();
            List<double> M_out = new List<double>();
            double Lb = double.NaN;
            double E = double.NaN;
            // output
            double M = double.NaN;
            double Sig = double.NaN;
            double D = double.NaN;
            //
            double L, Zy;
            double C = 1.0;
            double fb = 0.0;

            // 入力設定＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            if (!DA.GetDataList(0, Param)) { return; }
            if (!DA.GetData(1, ref M)) { return; }
            if (!DA.GetData(2, ref Lb)) { return; }
            if (!DA.GetData(3, ref E)) { return; }

            // 必要な引数の割り当て＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            L = Param[1];;
            Zy = Param[4];

            // 梁の計算箇所＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            Sig = M * 1000000 / Zy;
            D = 0;

            // モーメントの出力＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            M_out.Add(M);
            M_out.Add(M);
            M_out.Add(M);
            M_out.Add(M);
            M_out.Add(M);
            M_out.Add(L);

            // 許容曲げの計算 ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            Solver.Calc_fb_solver slv = new Solver.Calc_fb_solver();
            slv.Calc_fb( Param, Lb, C, ref fb);

            // 出力設定＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            DA.SetDataList(0, M_out);
            DA.SetData(1, Sig);
            DA.SetData(2, fb);
            DA.SetData(3, Sig / fb);
            DA.SetData(4, D);
        }
        /// <summary>
        /// アイコンの設定。24x24 pixelsが推奨
        /// </summary>
        protected override System.Drawing.Bitmap Icon {
            get {
                return BeamAnalysis.Properties.Resource.AnyM_icon;
            }
        }
        /// <summary>
        /// GUIDの設定
        /// </summary>
        public override Guid ComponentGuid {
            get {
                return new Guid("621eac11-23fb-445c-9430-44ce37ba9020");
            }
        }
    }
}
/// <summary>
/// rhino上への出力と断面諸元の計算
/// </summary>
namespace ModelDisp {
    /// <summary>
    /// H型断面の計算、出力
    /// </summary>
    public class H_Shape_Model : GH_Component {
        private List<Brep> ModelBrep = new List<Brep>();
        private Rhino.Display.DisplayMaterial ModelMaterial;
        private Color ModelColour = Color.FromName("LightCoral");

        public H_Shape_Model()
            : base("Make H Shape Model", "H Shape", "Display H Shape Model", "rgkr", "CrossSection") {
        }
        public override void ClearData() {
            base.ClearData();
            ModelBrep.Clear();
        }
        protected override void BeforeSolveInstance() {
            ModelBrep = new List<Brep>();
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager) {
            pManager.AddNumberParameter("Width", "B", "Model Width (mm)", GH_ParamAccess.item, 200.0);
            pManager.AddNumberParameter("Height", "H", "Model High (mm)", GH_ParamAccess.item, 400.0);
            pManager.AddNumberParameter("Web Thickness", "tw", "Web Thickness (mm)", GH_ParamAccess.item, 8.0);
            pManager.AddNumberParameter("Flange Thickness", "tf", "Flange Thickness (mm)", GH_ParamAccess.item, 13.0);
            pManager.AddNumberParameter("F", "F", "F (N/mm2)", GH_ParamAccess.item, 235);
            pManager.AddNumberParameter("Length", "L", "Model Length (mm)", GH_ParamAccess.item, 6300.0);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
            pManager.AddNumberParameter("Analysis Parametar", "Param", "output Analysis Parameter", GH_ParamAccess.item);
            pManager.AddSurfaceParameter("View Model Surface", "Srf", "output Model Surface", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA) {
            // 引数設定
            double B = double.NaN;
            double H = double.NaN;
            double L = double.NaN;
            double tw = double.NaN;
            double tf = double.NaN;
            double F = double.NaN;
            double Iy, Zy, i_t, lamda, Af;
            int fb_calc = 0; // 0:H強軸　1:箱型、丸形　2:L形

            // 入力設定
            if (!DA.GetData(0, ref B)) { return; }
            if (!DA.GetData(1, ref H)) { return; }
            if (!DA.GetData(2, ref tw)) { return; }
            if (!DA.GetData(3, ref tf)) { return; }
            if (!DA.GetData(4, ref F)) { return; }
            if (!DA.GetData(5, ref L)) { return; }

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

            // 許容曲げ関連の計算
            i_t = Math.Sqrt((tf * B * B * B + (H / 6.0 - tf) * tw * tw * tw) / (12 * (tf * B + (H / 6.0 - tf) * tw)));
            lamda = 1500 / Math.Sqrt(F / 1.5);
            Af = B * tf;

            // ひとまとめにするため List で作成
            List<double> Params = new List<double>();
            Params.Add(H);
            Params.Add(L);  //  部材長さ
            Params.Add(F);
            Params.Add(Iy); //  断面二次モーメント
            Params.Add(Zy); //  断面係数
            Params.Add(fb_calc); 
            Params.Add(i_t);
            Params.Add(lamda);
            Params.Add(Af);

            // モデルはRhino上に出力するだけなので、とりあえず配列でまとめる
            var model = new PlaneSurface[3];
            model[0] = upper_flange;
            model[1] = bottom_flange;
            model[2] = web;
            ModelBrep.Add(upper_flange.ToBrep());
            ModelBrep.Add(bottom_flange.ToBrep());
            ModelBrep.Add(web.ToBrep());

            // まとめての出力なので、SetDataList で出力
            DA.SetDataList(1, model);
            DA.SetDataList(0, Params);
        }
        /// <summary>
        /// Rhino の viewport への出力
        /// </summary>
        public override void DrawViewportWires(IGH_PreviewArgs args) {
            ModelMaterial = new Rhino.Display.DisplayMaterial(ModelColour);
            if (ModelBrep != null) {
                for (int i = 0; i < 3; i++) {
                    args.Display.DrawBrepShaded(ModelBrep[i], ModelMaterial);
                }
            }
        }
        public override void DrawViewportMeshes(IGH_PreviewArgs args) {
            ModelMaterial = new Rhino.Display.DisplayMaterial(ModelColour);
            if (ModelBrep != null) {
                for (int i = 0; i < 3; i++) {
                    args.Display.DrawBrepWires(ModelBrep[i], ModelMaterial.Diffuse, 0);
                }
            }
        }
        protected override System.Drawing.Bitmap Icon {
            get {
                return BeamAnalysis.Properties.Resource.H_icon;
            }
        }
        public override Guid ComponentGuid {
            get {
                return new Guid("419c3a3a-cc48-4717-8cef-5f5647a5ecAa");
            }
        }
    }
    /// <summary>
    /// L型断面の計算、出力
    /// </summary>
    public class L_Shape_Model : GH_Component {
        public L_Shape_Model()
            : base("Make L Shape Model",
                   "L Shape",
                   "Display L Shape Model",
                   "rgkr",
                   "CrossSection"
                  ) {
        }
        public override void ClearData() {
            base.ClearData();
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager) {
            pManager.AddNumberParameter("Width", "B", "Model Width (mm)", GH_ParamAccess.item, 75.0);
            pManager.AddNumberParameter("Height", "H", "Model High (mm)", GH_ParamAccess.item, 75.0);
            pManager.AddNumberParameter("Web Thickness", "tw", "Web Thickness (mm)", GH_ParamAccess.item, 9.0);
            pManager.AddNumberParameter("Flange Thickness", "tf", "Flange Thickness (mm)", GH_ParamAccess.item, 9.0);
            pManager.AddNumberParameter("F", "F", "F (N/mm2)", GH_ParamAccess.item, 235);
            pManager.AddNumberParameter("Length", "L", "Model Length (mm)", GH_ParamAccess.item, 3000.0);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
            pManager.AddNumberParameter("Analysis Parametar", "Param", "output Analysis Parameter", GH_ParamAccess.item);
            pManager.AddSurfaceParameter("View Model Surface", "Srf", "output Model Surface", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA) {
            // 引数設定
            double B = double.NaN;
            double H = double.NaN;
            double L = double.NaN;
            double tw = double.NaN;
            double tf = double.NaN;
            double F = double.NaN;
            double Iy, Zy, i_t, lamda, Af;
            int fb_calc = 2; // 0:H強軸　1:箱型、丸形　2:L形

            // 入力設定
            if (!DA.GetData(0, ref B)) { return; }
            if (!DA.GetData(1, ref H)) { return; }
            if (!DA.GetData(2, ref tw)) { return; }
            if (!DA.GetData(3, ref tf)) { return; }
            if (!DA.GetData(4, ref F)) { return; }
            if (!DA.GetData(5, ref L)) { return; }

            // 原点の作成
            var Ori = new Point3d(0, 0, 0);
            
            // フランジのサーフェス作成
            var Fp1 = new Point3d(0, 0, -H / 2);
            var Fp2 = new Point3d(1, 0, -H / 2);
            var Fp3 = new Point3d(0, 1, -H / 2);
            var Fplane = new Plane(Fp1, Fp2, Fp3);
            var flange = new PlaneSurface(Fplane, new Interval(-B / 2, B / 2), new Interval(0, L));

            // ウェブのサーフェス作成
            var Wp1 = new Point3d(-B / 2, 0, 0);
            var Wp2 = new Point3d(-B / 2, 0, -1);
            var Wp3 = new Point3d(-B / 2, 1, 0);
            var Wplane = new Plane(Wp1, Wp2, Wp3);
            var web = new PlaneSurface(Wplane, new Interval(-H / 2, H / 2), new Interval(0, L));

            // 解析用パラメータの計算
            Iy = (1.0 / 12.0 * B * H * H * H) - (1.0 / 12.0 * (B - tw) * (H - 2 * tf) * (H - 2 * tf) * (H - 2 * tf));
            Zy = Iy / (H / 2);

            // 許容曲げ関連の計算
            i_t = Math.Sqrt((tf * B * B * B + (H / 6.0 - tf) * tw * tw * tw) / (12 * (tf * B + (H / 6.0 - tf) * tw)));
            lamda = 1500 / Math.Sqrt(F / 1.5);
            Af = B * tf;

            // ひとまとめにするため List で作成
            List<double> Params = new List<double>();
            Params.Add(H);
            Params.Add(L);  //  部材長さ
            Params.Add(F);
            Params.Add(Iy); //  断面二次モーメント
            Params.Add(Zy); //  断面係数
            Params.Add(fb_calc);
            Params.Add(0);
            Params.Add(0);
            Params.Add(0);

            // モデルはRhino上に出力するだけなので、とりあえず配列でまとめる
            var model = new PlaneSurface[2];
            model[0] = flange;
            model[1] = web;

            // まとめての出力なので、SetDataList で出力
            DA.SetDataList(1, model);
            DA.SetDataList(0, Params);
        }
        protected override System.Drawing.Bitmap Icon {
            get {
                return BeamAnalysis.Properties.Resource.L_icon;
            }
        }
        public override Guid ComponentGuid {
            get {
                return new Guid("419c3a44-cc48-4717-8cef-5f5647a5ecAa");
            }
        }
    }
    /// <summary>
    /// 箱型断面の計算、出力
    /// </summary>
    public class BOX_Shape_Model : GH_Component {
        public BOX_Shape_Model()
            : base("Make BOX Shape Model",
                   "BOX Shape",
                   "Display BOX Shape Model",
                   "rgkr",
                   "CrossSection"
                  ) {
        }
        public override void ClearData() {
            base.ClearData();
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager) {
            pManager.AddNumberParameter("Width", "B", "Model Width (mm)", GH_ParamAccess.item, 150.0);
            pManager.AddNumberParameter("Height", "H", "Model High (mm)", GH_ParamAccess.item, 150.0);
            pManager.AddNumberParameter("Thickness", "t", "Thickness (mm)", GH_ParamAccess.item, 6.0);
            pManager.AddNumberParameter("F", "F", "F (N/mm2)", GH_ParamAccess.item, 235);
            pManager.AddNumberParameter("Length", "L", "Model Length (mm)", GH_ParamAccess.item, 3200.0);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
            pManager.AddNumberParameter("Analysis Parametar", "Param", "output Analysis Parameter", GH_ParamAccess.item);
            pManager.AddSurfaceParameter("View Model Surface", "Srf", "output Model Surface", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA) {
            // 引数設定
            double B = double.NaN;
            double H = double.NaN;
            double L = double.NaN;
            double t = double.NaN;
            double F = double.NaN;
            double Iy, Zy;
            int fb_calc = 1; // 0:H強軸　1:箱型、丸形　2:L形

            // 入力設定
            if (!DA.GetData(0, ref B)) { return; }
            if (!DA.GetData(1, ref H)) { return; }
            if (!DA.GetData(2, ref t)) { return; }
            if (!DA.GetData(3, ref F)) { return; }
            if (!DA.GetData(4, ref L)) { return; }

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
            var LWp1 = new Point3d(-B / 2, 0, 0);
            var LWp2 = new Point3d(-B / 2, 0, -1);
            var LWp3 = new Point3d(-B / 2, 1, 0);
            var LWplane = new Plane(LWp1, LWp2, LWp3);
            var Lweb = new PlaneSurface(LWplane, new Interval(-H / 2, H / 2), new Interval(0, L));

            // ウェブのサーフェス作成
            var RWp1 = new Point3d(B / 2, 0, 0);
            var RWp2 = new Point3d(B / 2, 0, -1);
            var RWp3 = new Point3d(B / 2, 1, 0);
            var RWplane = new Plane(RWp1, RWp2, RWp3);
            var Rweb = new PlaneSurface(RWplane, new Interval(-H / 2, H / 2), new Interval(0, L));

            // 解析用パラメータの計算
            Iy = 1.0 / 12.0 * ((B * H * H * H) - ((B - t) * (H - t) * (H - t) * (H - t)));
            Zy = Iy / (H / 2);

            // ひとまとめにするため List で作成
            List<double> Params = new List<double>();
            Params.Add(H);
            Params.Add(L);  //  部材長さ
            Params.Add(F);
            Params.Add(Iy); //  断面二次モーメント
            Params.Add(Zy); //  断面係数
            Params.Add(fb_calc);
            Params.Add(0);
            Params.Add(0);
            Params.Add(0);

            // モデルはRhino上に出力するだけなので、とりあえず配列でまとめる
            var model = new PlaneSurface[4];
            model[0] = upper_flange;
            model[1] = bottom_flange;
            model[2] = Rweb;
            model[3] = Lweb;

            // まとめての出力なので、SetDataList で出力
            DA.SetDataList(0, Params);
            DA.SetDataList(1, model);
        }
        protected override Bitmap Icon {
            get {
                return BeamAnalysis.Properties.Resource.BOX_icon;
            }
        }
        public override Guid ComponentGuid {
            get {
                return new Guid("419c3a44-cc48-4717-8fdf-5f5647a5ecAa");
            }
        }
    }
}
/// <summary>
/// rhino上への結果の出力関連の設定
/// </summary>
namespace ResultView {
    public class MomentViewer : GH_Component {
        private List<double> M = new List<double>();
        private double M1 = 0, M2 = 0, M3 = 0, M4 = 0, M5 = 0, L = 0;
        private Point3d M_point1, M_point2, M_point3, M_point4, M_point5;
        private List<Brep> MomentBrep = new List<Brep> ();
        private Rhino.Display.DisplayMaterial MomentMaterial;
        private Color TextColour = Color.FromName("Black");
        private Color MomentColour = Color.FromName("SkyBlue");

        public MomentViewer()
            : base("Moment View", "Moment", "Display Moment", "rgkr", "Result") {
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager) {
            pManager.AddNumberParameter("Moment", "M", "Input Moment", GH_ParamAccess.list);
            pManager.AddNumberParameter("Scale", "Sc", "Input Output Scale", GH_ParamAccess.item, 10);
        }
        public override void ClearData() {
            base.ClearData();
            M.Clear();
            MomentBrep.Clear();
        }
        protected override void BeforeSolveInstance() {
            M = new List<double>();
            MomentBrep = new List<Brep>();
            M1 = 0; M2 = 0; M3 = 0; M4 = 0; M5 = 0; L = 0;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
            pManager.AddBrepParameter("View Moment Surface", "Srf", "output Moment Surface", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA) {
            // 引数設定
            double Sc = double.NaN;

            // 入力設定
            if (!DA.GetDataList(0, M)) { return; }
            if (!DA.GetData(1, ref Sc)) { return; }

            M1 = M[0];
            M2 = M[1];
            M3 = M[2];
            M4 = M[3];
            M5 = M[4];
            L = M[5];

            // モーメント図の作成
            var M12_P1 = new Point3d(0, 0, 0);
            var M12_P2 = new Point3d(0, 0, Sc * -M1);
            var M12_P3 = new Point3d(0, L / 4, Sc * -M2);
            var M12_P4 = new Point3d(0, L / 4, 0);
            M_point1 = new Point3d(0, 0, Sc * -M1);
            //
            var M23_P1 = new Point3d(0, L / 4, 0);
            var M23_P2 = new Point3d(0, L / 4, Sc * -M2);
            var M23_P3 = new Point3d(0, L / 2, Sc * -M3);
            var M23_P4 = new Point3d(0, L / 2, 0);
            M_point2 = new Point3d(0, L / 4, Sc * -M2);
            //
            var M34_P1 = new Point3d(0, L / 2, 0);
            var M34_P2 = new Point3d(0, L / 2, Sc * -M3);
            var M34_P3 = new Point3d(0, 3 * L / 4, Sc * -M4);
            var M34_P4 = new Point3d(0, 3 * L / 4, 0);
            M_point3 = new Point3d(0, L / 2, Sc * -M3);
            //
            var M45_P1 = new Point3d(0, 3 * L / 4, 0);
            var M45_P2 = new Point3d(0, 3 * L / 4, Sc * -M4);
            var M45_P3 = new Point3d(0, L, Sc * -M5);
            var M45_P4 = new Point3d(0, L, 0);
            M_point4 = new Point3d(0, 3 * L / 4, Sc * -M4);
            M_point5 = new Point3d(0, L, Sc * -M5);

            Brep M12_brep = Brep.CreateFromCornerPoints(M12_P1, M12_P2, M12_P3, M12_P4, GH_Component.DocumentTolerance());
            Brep M23_brep = Brep.CreateFromCornerPoints(M23_P1, M23_P2, M23_P3, M23_P4, GH_Component.DocumentTolerance());
            Brep M34_brep = Brep.CreateFromCornerPoints(M34_P1, M34_P2, M34_P3, M34_P4, GH_Component.DocumentTolerance());
            Brep M45_brep = Brep.CreateFromCornerPoints(M45_P1, M45_P2, M45_P3, M45_P4, GH_Component.DocumentTolerance());

            // モデルはRhino上に出力するだけなので、とりあえず配列でまとめる
            MomentBrep.Add(M12_brep);
            MomentBrep.Add(M23_brep);
            MomentBrep.Add(M34_brep);
            MomentBrep.Add(M45_brep);

            // まとめての出力なので、SetDataList で出力
            DA.SetDataList(0, MomentBrep);
        }
        /// <summary>
        /// Rhino の viewport への出力
        /// </summary>
        public override void DrawViewportWires(IGH_PreviewArgs args) {
            if (M2 != 0) {
                args.Display.Draw2dText(M1.ToString("F1"), TextColour, M_point1, true, 22);
                args.Display.Draw2dText(M2.ToString("F1"), TextColour, M_point2, true, 22);
                args.Display.Draw2dText(M3.ToString("F1"), TextColour, M_point3, true, 22);
                args.Display.Draw2dText(M4.ToString("F1"), TextColour, M_point4, true, 22);
                args.Display.Draw2dText(M5.ToString("F1"), TextColour, M_point5, true, 22);
            }
            // sureface 色付け
            MomentMaterial = new Rhino.Display.DisplayMaterial(MomentColour);

            if (M2 != 0) {
                for (int i = 0; i < 4; i++) {
                    args.Display.DrawBrepShaded(MomentBrep[i], MomentMaterial);
                }
            }
        }
        public override void DrawViewportMeshes(IGH_PreviewArgs args) {
            MomentMaterial = new Rhino.Display.DisplayMaterial(MomentColour);

            if (M2 != 0) {
                for (int i = 0; i < 4; i++) {
                    args.Display.DrawBrepWires(MomentBrep[i], MomentMaterial.Diffuse, 0);
                }
            }
        }
        protected override System.Drawing.Bitmap Icon {
            get {
                return BeamAnalysis.Properties.Resource.Result_M_icon;
            }
        }
        public override Guid ComponentGuid {
            get {
                return new Guid("419c3a3a-cc48-4717-8cef-5f5647a5dcAa");
            }
        }
    }
}
/// <summary>
/// 解析関連
/// </summary>
namespace Solver {
    /// <summary>
    /// 許容曲げを計算するクラス
    /// </summary>
    public class Calc_fb_solver {
        public void Calc_fb(List<double> Param, double Lb,  double C, ref double fb) {
            // 解析関連パラメータ ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            double H, L, F, fb_calc, i_t, lamda, Af, fb1, fb2;
            H = Param[0];
            L = Param[1];
            F = Param[2];
            fb_calc = Param[5];
            i_t = Param[6];
            lamda = Param[7];
            Af = Param[8];

            // 計算箇所 ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            // H強軸回りの場合
            if (fb_calc == 0) {
                fb1 = (1.0 - 0.4 * (Lb / i_t) * (Lb / i_t) / (C * lamda * lamda)) * F / 1.5;
                fb2 = 89000.0 / (Lb * H / Af);
                fb = Math.Min(Math.Max(fb1, fb2), F / 1.5);
            }
            // 箱型丸型の場合
            else if (fb_calc == 1) {
                fb = F / 1.5;
            }
            // L型等非対称断面の場合
            else if (fb_calc == 2) {
                fb2 = 89000.0 / (Lb * H / Af);
                fb = Math.Min(fb2, F / 1.5);
            }
            // エラー用　sig/fb が inf になるように 0指定
            else {
                fb = 0.0;
            }
        }
    }
}
/// <summary>
/// UI にかかわる設定
/// </summary>
namespace UI_Setting {
    /// <summary>
    /// ボタンを押すとWindowsFormを出力させる
    /// </summary>
    public class Attributes_Custom : GH_ComponentAttributes {
        public Attributes_Custom(GH_Component owner) : base(owner) {
        }
        /// <summary>
        /// ボタンの箱(Rectangle)をを設定する。
        /// サイズが直接指定なので、汎用性は低め
        /// 今後は引数から箱のサイズを決めれるようにしたい。
        /// </summary>
        protected override void Layout() {
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
        private Rectangle ButtonBounds {
            get;
            set;
        }
        /// <summary>
        /// ラジオボタンの作りかけ
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="center"></param>
        /// <param name="Checked"></param>
        private void DrawRadioButton(Graphics graphics, PointF center, bool Checked ) {
            if (Checked) {
                graphics.FillEllipse(Brushes.Black, center.X - 6, center.Y - 6, 12, 12);
            }
            else {
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
        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel) {
            base.Render(canvas, graphics, channel);
            if (channel == GH_CanvasChannel.Objects) {
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
        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e) {
            if (e.Button == MouseButtons.Left) {
                RectangleF rec = ButtonBounds;
                if (rec.Contains(e.CanvasLocation)) {
                    MessageBox.Show("入力データと解析結果を\nWindowsFormを使用して出力させる。", "結果のアウトプット", MessageBoxButtons.OK);
                    return GH_ObjectResponse.Handled;
                }
            }            
            return base.RespondToMouseDown(sender, e);
        }
    }
}
/// <summary>
/// 開発中、テスト中のものを入れる名前空間
/// </summary>
namespace WIP{
    public class test_Component : GH_Component {
        private string m_T;
        private Color colour;
        private Point3d m_P;

        public override Guid ComponentGuid {
            get {
                return new Guid("{3B220754-4114-4170-B6C3-B286B86ED511}");
            }
        }
        public test_Component() : base("test", "test", "test text tags in a Rhino viewport", "rgkr", "wip") {
        }
        public override void ClearData() {
            base.ClearData();
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddPointParameter("Location", "L", "Location of text tag", GH_ParamAccess.item);
            pManager.AddTextParameter("Text", "T", "The text to display", GH_ParamAccess.item);
            pManager.AddColourParameter("Colour", "C", "Optional colour for tag", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
        }
        protected override void SolveInstance(IGH_DataAccess DA)  {
            if (!DA.GetData(0, ref m_P)) { return; }
            if (!DA.GetData(1, ref m_T)) { return; }
            if (!DA.GetData(2, ref colour)) { return; }
        }
        public override void DrawViewportWires(IGH_PreviewArgs args) {
            //  string m_T = "test";
            //  Color colour = Color.FromName("SlateBlue");
            //  Point3d m_P = new Point3d(0,0,0);
            args.Display.Draw2dText(m_T, colour, m_P, true, 22);
        }
    }
    public class test_ColourComponent : GH_Component {
        private Brep m_T;
        private Color colour;
        private Rhino.Display.DisplayMaterial material;

        public override Guid ComponentGuid {
            get {
                return new Guid("{3B220754-4114-4170-B6C3-B286B86ED501}");
            }
        }
        public test_ColourComponent() : base("testColour", "testColour", "test text tags in a Rhino viewport", "rgkr", "wip") {
        }
        public override void ClearData() {
            base.ClearData();
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddBrepParameter("Location", "B", "Location of text tag", GH_ParamAccess.item);
            pManager.AddColourParameter("Colour", "C", "Optional colour for tag", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
        }
        protected override void SolveInstance(IGH_DataAccess DA) {
            if (!DA.GetData(0, ref m_T)) { return; }
            if (!DA.GetData(1, ref colour)) { return; }
            material = new Rhino.Display.DisplayMaterial(colour);
        }
        public override void DrawViewportMeshes(IGH_PreviewArgs args) {
             args.Display.DrawBrepWires(m_T, material.Diffuse, 0);
        }
        public override void DrawViewportWires(IGH_PreviewArgs args) {
             args.Display.DrawBrepShaded(m_T, material);
        }
    }
}