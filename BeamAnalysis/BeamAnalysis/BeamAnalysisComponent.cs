using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
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
          : base("BeamAnalysis", "BeamAnalysis",
              "analyzes the beam",
              "rgkr", "beam")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Analysis Parametar", "Param", "Input Analysis Parameter", GH_ParamAccess.list);
            pManager.AddNumberParameter("Load", "P", "Centralized load (kN)", GH_ParamAccess.item);
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
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("621eac03-23fb-445c-9430-44ce37bf9020"); }
        }
    }
}
namespace H_Shape_Model
{
    public class H_Shape_Model : GH_Component
    {

        public H_Shape_Model() : base("Make H Shape Model", "H Steel", "Display H Shape Model", "rgkr", "H-Shape")
        {
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Width", "B", "Model Width (mm)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "H", "Model High (mm)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Web Thickness", "tw", "Web Thickness (mm)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Flange Thickness", "tf", "Flange Thickness (mm)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Length", "L", "Model Length (mm)", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddSurfaceParameter("View Model", "model", "output Model", GH_ParamAccess.item);
            pManager.AddNumberParameter("Analysis Parametar", "Param", "output Analysis Parameter", GH_ParamAccess.item);
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

            // 入力設定
            if (!DA.GetData(0, ref B)) { return; }
            if (!DA.GetData(1, ref H)) { return; }
            if (!DA.GetData(2, ref tw)) { return; }
            if (!DA.GetData(3, ref tf)) { return; }
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
