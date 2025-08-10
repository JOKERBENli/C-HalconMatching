using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TemplateMatching
{
    public partial class Form1 : Form
    {
        //创建存储ROI的集合对象
        private List<DrawingObjectEx> drawingObjectExs = new List<DrawingObjectEx>();
        private HSmartWindowControl hwControl;
        private HImage ho_Image;
        private HTuple hv_Width;
        private HTuple hv_Height;
        private HTuple hv_Row;
        private HTuple hv_Column;
        private HTuple hv_ModelID;
        private HObject ho_ModelContours;
        private HTuple hv_HomMat2DIdentity;
        private HObject ho_Rectangle;

        public Form1()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 窗体加载的时候初始化添加Halcon控件到容器中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            //创建一个halcon控件对象
            hwControl = new HSmartWindowControl();
            //设置控件以填充方式放入到父容器中
            hwControl.Dock = DockStyle.Fill;
            //添加到对应容器中
            panel1.Controls.Add(hwControl);


        }   
        private void button1_Click(object sender, EventArgs e)
        {
            //创建一个资源对话框对象
            var dialog = new OpenFileDialog();
            dialog.Filter = "选择模板图像|*.png";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                //获取对话框中选择的资源路径
                var filePath = dialog.FileName;
                //通过路径创建一个Halcon图像对象
                ho_Image = new HImage(filePath);
                //把图像显示到halcon窗口上
                hwControl.HalconWindow.DispObj(ho_Image);

                //设置让其图像适应容器大小（等比例缩放不失真）
                hwControl.SetFullImagePart();
            }

        }

        /// <summary>
        /// 绘制模板ROI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            //提供绘制ROI的数据数组
            var roiData = new HTuple[] { 100,150,150,250 };
            //创建矩形ROI对象
            var rectROI = HDrawingObject.CreateDrawingObject(HDrawingObject.HDrawingObjectType.RECTANGLE1, roiData);
            //注册ROI对象的拖拽及缩放事件
            rectROI.OnDrag(Rect1OnDragAction);
            rectROI.OnResize(Rect1OnResizeAction);

            //把绘制ROI对象、数据，ROI类型保存起来
            drawingObjectExs.Add(new DrawingObjectEx
            {
                DrawingObject = rectROI,
                DrawingObjectDatas = roiData,
                DrawingObjectType = HDrawingObject.HDrawingObjectType.RECTANGLE1

            });
            //把创建ROI附加到halcon窗口中显示
            hwControl.HalconWindow.AttachDrawingObjectToWindow(rectROI);

        }

        /// <summary>
        /// 绘制测量ROI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            //判断模板ROI是否绘制
            if(drawingObjectExs.Count == 0)
            {
                MessageBox.Show("请先绘制模板ROI后再绘制测量ROI");
                return;
            }

            //提供绘制ROI的数据数组
            var roiData = new HTuple[] { 200, 350 , 0, 163, 7 };
            //创建矩形ROI对象
            var rectROI = HDrawingObject.CreateDrawingObject(HDrawingObject.HDrawingObjectType.RECTANGLE2, roiData);
            //注册ROI对象的拖拽及缩放事件
            rectROI.OnDrag(Rect2OnDragAction);
            rectROI.OnResize(Rect2OnResizeAction);

            //把绘制ROI对象、数据，ROI类型保存起来
            drawingObjectExs.Add(new DrawingObjectEx
            {
                DrawingObject = rectROI,
                DrawingObjectDatas = roiData,
                DrawingObjectType = HDrawingObject.HDrawingObjectType.RECTANGLE2

            });
            //把创建ROI附加到halcon窗口中显示
            hwControl.HalconWindow.AttachDrawingObjectToWindow(rectROI);
        }

        private void Rect1OnDragAction(HDrawingObject drawid,HWindow window,string type)
        {
            //更新ROI对象的数据
            UpdateROIData(drawid, 0);

        }
        
        private void Rect1OnResizeAction(HDrawingObject drawid, HWindow window, string type)
        {
            //更新ROI对象的数据
            UpdateROIData(drawid, 0);


        }
        private void Rect2OnDragAction(HDrawingObject drawid, HWindow window, string type)
        {
            //更新ROI对象的数据
            UpdateROIData(drawid, 1);

        }

        private void Rect2OnResizeAction(HDrawingObject drawid, HWindow window, string type)
        {
            //更新ROI对象的数据
            UpdateROIData(drawid, 1);


        }
        /// <summary>
        /// 更新ROI数据的方法
        /// </summary>
        /// <param name="drawid"></param>
        /// <param name="v"></param>
        private void UpdateROIData(HDrawingObject drawid, int index)
        {
            //获取需要处理的ROI对象
            var roi = drawingObjectExs[index];
            //获取ROI类型
            var roiType = roi.DrawingObjectType;
           
            HTuple[] valueArray = null;
            switch (roiType)
            {
                case HDrawingObject.HDrawingObjectType.RECTANGLE1:
                    //定义矩形的参数名称对象
                    var nameTuple = new HTuple("row1", "column1", "row2", "column2");
                    //获取对应参数名称组成数据元组
                    var valueTuple = drawid.GetDrawingObjectParams(nameTuple);
                    //组织对应数据数组
                    valueArray = new HTuple[] { valueTuple[0], valueTuple[1], valueTuple[2], valueTuple[3] };
                    break;
                case HDrawingObject.HDrawingObjectType.RECTANGLE2:

                    //定义矩形的参数名称对象
                    nameTuple = new HTuple("row", "column", "phi", "length1", "length2");
                    //获取对应参数名称组成数据元组
                    valueTuple = drawid.GetDrawingObjectParams(nameTuple);
                    //组织对应数据数组
                    valueArray = new HTuple[] { valueTuple[0], valueTuple[1], valueTuple[2], valueTuple[3], valueTuple[4] };
                    break;
            }
            //更新ROI对象
            roi.DrawingObjectDatas = valueArray;


        }

        /// <summary>
        /// 模板创建
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            //* 获取模板ROI对象及测量ROI对象
            var templateROI = drawingObjectExs[0].DrawingObjectDatas;
            var mesuresROI = drawingObjectExs[1].DrawingObjectDatas;

            HOperatorSet.GetImagePointer1(ho_Image, out HTuple hv_Pointer, out HTuple hvType, out hv_Width, out hv_Height);

            //* 获取创建模板的区域

            HOperatorSet.GenRectangle1(out HObject ho_ROI_0, templateROI[0].D, templateROI[1].D, templateROI[2].D, templateROI[3].D);

            //* 获取矩形的中心坐标

            HOperatorSet.AreaCenter(ho_ROI_0, out HTuple hv_Area, out hv_Row, out hv_Column);
            //* 显示图像的模板区域图像

            HOperatorSet.ReduceDomain(ho_Image, ho_ROI_0, out HObject ho_ImageReduced);
            
            HOperatorSet.CreateShapeModel(ho_ImageReduced, 4, 0
                , 6.29, "auto", "none", "use_polarity", (new HTuple(25)).TupleConcat(
                40), 10, out hv_ModelID);
          
            //* 获取创建模板的轮廓

            HOperatorSet.GetShapeModelContours(out ho_ModelContours, hv_ModelID, 1);
            //* 生成一个仿射变换矩阵
            HOperatorSet.HomMat2dIdentity(out hv_HomMat2DIdentity);
            
            //* 生成测量跟随的轮廓

            HOperatorSet.GenRectangle2ContourXld(out ho_Rectangle, mesuresROI[0].D, mesuresROI[1].D, mesuresROI[2].D, mesuresROI[3].D, mesuresROI[4].D);
            //持久化模板及轮廓及跟随测量ROI
            HOperatorSet.WriteShapeModel(hv_ModelID, "model");
            HOperatorSet.WriteObject(ho_ModelContours, "model_xld");
            HOperatorSet.WriteObject(ho_Rectangle,"check_xld");
            //清空ROI对象
            ClearROI();
            MessageBox.Show("模板创建成功");

        }

        /// <summary>
        /// 清空ROI集合并卸载roi对象
        /// </summary>
        private void ClearROI()
        {
            foreach(var item in drawingObjectExs)
            {
                var roi = item.DrawingObject;
                //卸载ROI
                hwControl.HalconWindow.DetachDrawingObjectFromWindow(roi);
                roi.Dispose();
                roi = null;

            }
            //清空ROI集合
            drawingObjectExs.Clear();
        }
        /// <summary>
        /// 加载测量的图像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            //创建一个资源对话框对象
            var dialog = new OpenFileDialog();
            dialog.Filter = "选择待测量图像|*.png";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                //获取对话框中选择的资源路径
                var filePath = dialog.FileName;
                //通过路径创建一个Halcon图像对象
                ho_Image = new HImage(filePath);
                //把图像显示到halcon窗口上
                hwControl.HalconWindow.DispObj(ho_Image);

                //设置让其图像适应容器大小（等比例缩放不失真）
                hwControl.SetFullImagePart();
            }
        }
        /// <summary>
        /// 加载模板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            //加载模板对象
            HOperatorSet.ReadShapeModel("model",out hv_ModelID);
            //加载轮廓
            HOperatorSet.ReadObject(out ho_ModelContours,"model_xld");
            //加载测量区域
            MessageBox.Show("模板加载成功");

        }

        /// <summary>
        /// 执行测量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            HOperatorSet.FindShapeModel(ho_Image, hv_ModelID, 0, (new HTuple(360)).TupleRad()
                      , 0.5, 1, 0.5, "least_squares", 0, 0.7, out HTuple hv_Row1, out HTuple hv_Column1,
                      out HTuple hv_Angle, out HTuple hv_Score);
            //模板仿射变换
            // 生成一个仿射变换矩阵
            HOperatorSet.HomMat2dIdentity(out hv_HomMat2DIdentity);
            HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_Row1, hv_Column1,
                out HTuple hv_HomMat2DTranslate);
            HOperatorSet.HomMat2dRotate(hv_HomMat2DTranslate, hv_Angle, hv_Row1, hv_Column1,
                out HTuple hv_HomMat2DRotate);
            HOperatorSet.AffineTransContourXld(ho_ModelContours, out HObject ho_ContoursAffineTrans,
                hv_HomMat2DRotate);
            //跟随1,使用vector_angle_to_rigid算子 刚体变换
            HOperatorSet.VectorAngleToRigid(hv_Row, hv_Column, 0, hv_Row1, hv_Column1,
                hv_Angle, out HTuple hv_HomMat2D);
            HOperatorSet.AffineTransContourXld(ho_Rectangle, out HObject ho_ContoursAffineTrans1,
                hv_HomMat2D);
            HOperatorSet.AreaCenterXld(ho_ContoursAffineTrans1, out HTuple hv_Area1, out HTuple hv_Row2,
                out HTuple hv_Column2, out HTuple hv_PointOrder);
            HOperatorSet.DispObj(ho_Image, hwControl.HalconWindow);
            HOperatorSet.DispObj(ho_ContoursAffineTrans, hwControl.HalconWindow);
            HOperatorSet.DispObj(ho_ContoursAffineTrans1, hwControl.HalconWindow);
            //测量1
            HOperatorSet.GenMeasureRectangle2(hv_Row2, hv_Column2, hv_Angle, 162, 7,
                hv_Width, hv_Height, "nearest_neighbor", out HTuple hv_MeasureHandle);
            HOperatorSet.MeasurePairs(ho_Image, hv_MeasureHandle, 3, 50, "all", "all",
                out HTuple hv_RowEdgeFirst, out HTuple hv_ColumnEdgeFirst, out HTuple hv_AmplitudeFirst, out HTuple hv_RowEdgeSecond,
                out HTuple hv_ColumnEdgeSecond, out HTuple hv_AmplitudeSecond, out HTuple hv_IntraDistance,
                out HTuple hv_InterDistance);
            //显示
            HOperatorSet.DispLine(hwControl.HalconWindow, hv_RowEdgeFirst - (8 * (hv_Angle.TupleCos())), hv_ColumnEdgeFirst - (8 * (hv_Angle.TupleSin())), hv_RowEdgeFirst + (8 * (hv_Angle.TupleCos())), hv_ColumnEdgeFirst + (8 * (hv_Angle.TupleSin())));
            HOperatorSet.DispLine(hwControl.HalconWindow, hv_RowEdgeSecond - (8 * (hv_Angle.TupleCos())), hv_ColumnEdgeSecond - (8 * (hv_Angle.TupleSin())), hv_RowEdgeSecond + (8 * (hv_Angle.TupleCos())), hv_ColumnEdgeSecond + (8 * (hv_Angle.TupleSin())));
            //下引角
            //仿射变换跟随2,使用hom_mat2d_translate,hom_mat2d_rotate,affine_trans_pixel
            //先平移矩阵,再旋转矩阵.基于0,0点
            HOperatorSet.HomMat2dTranslate(hv_HomMat2DIdentity, hv_Row1 + 104, hv_Column1 + 6,
                    out HTuple hv_HomMat2DTranslate1);

            HOperatorSet.HomMat2dRotate(hv_HomMat2DTranslate1, hv_Angle, hv_Row1, hv_Column1,
                out HTuple hv_HomMat2DRotate1);

            HOperatorSet.AffineTransPixel(hv_HomMat2DRotate1, 0, 0, out HTuple hv_RowTrans,
                out HTuple hv_ColTrans);

            HOperatorSet.GenRectangle2ContourXld(out HObject ho_Rectangle1, hv_RowTrans, hv_ColTrans,
                hv_Angle, 163, 7);
            //测量2

            HOperatorSet.GenMeasureRectangle2(hv_RowTrans, hv_ColTrans, hv_Angle, 162,
                7, hv_Width, hv_Height, "nearest_neighbor", out HTuple hv_MeasureHandle1);

            HOperatorSet.MeasurePairs(ho_Image, hv_MeasureHandle1, 3, 50, "all", "all",
                out HTuple hv_RowEdgeFirst1, out HTuple hv_ColumnEdgeFirst1, out HTuple hv_AmplitudeFirst1,
                out HTuple hv_RowEdgeSecond1, out HTuple hv_ColumnEdgeSecond1, out HTuple hv_AmplitudeSecond1,
                out HTuple hv_IntraDistance1, out HTuple hv_InterDistance1);
            HOperatorSet.DispLine(hwControl.HalconWindow, hv_RowEdgeFirst1 - (8 * (hv_Angle.TupleCos()
                )), hv_ColumnEdgeFirst1 - (8 * (hv_Angle.TupleSin())), hv_RowEdgeFirst1 + (8 * (hv_Angle.TupleCos()
                )), hv_ColumnEdgeFirst1 + (8 * (hv_Angle.TupleSin())));
            HOperatorSet.DispLine(hwControl.HalconWindow, hv_RowEdgeSecond1 - (8 * (hv_Angle.TupleCos()
                )), hv_ColumnEdgeSecond1 - (8 * (hv_Angle.TupleSin())), hv_RowEdgeSecond1 + (8 * (hv_Angle.TupleCos()
                )), hv_ColumnEdgeSecond1 + (8 * (hv_Angle.TupleSin())));

            var hv_distancel = ((hv_IntraDistance.TupleConcat(hv_IntraDistance1))).TupleMean();
            var hv_distancelMin = ((hv_IntraDistance.TupleConcat(hv_IntraDistance1))).TupleMin();
            HOperatorSet.DispText(hwControl.HalconWindow, ("引角间隔平均值为:" + (hv_distancel.TupleString(
                "0.4"))) + "px", "window", 12, 12, "black", new HTuple(), new HTuple());
            HOperatorSet.DispText(hwControl.HalconWindow, ("引角间隔最小为:" + (hv_distancelMin.TupleString(
                "0.4"))) + "px", "window", 42, 12, "black", new HTuple(), new HTuple());
            HOperatorSet.DispText(hwControl.HalconWindow, new HTuple("引角总数为") + (new HTuple(((hv_IntraDistance.TupleConcat(
                        hv_IntraDistance1))).TupleLength())), "window", 72, 12, "black", new HTuple(),
                        new HTuple());
            //及时释放句柄空间
            HOperatorSet.CloseMeasure(hv_MeasureHandle);
            HOperatorSet.CloseMeasure(hv_MeasureHandle1);
            // 是否模板对象
            HOperatorSet.ClearShapeModel(hv_ModelID);
            }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }

    //ROI的扩展类，主要封装ROI对象本身及ROI对应数据和绘制的ROI类型
    class DrawingObjectEx
    {
        //ROI对象
        public HDrawingObject DrawingObject { get; set; }
        //ROI绘制时候的数据
        public HTuple[] DrawingObjectDatas { get; set; }
        //绘制的ROI类型，圆形，椭圆，矩形
        public HDrawingObject.HDrawingObjectType DrawingObjectType { get; set; } 
    }

}
