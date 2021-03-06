﻿using System;
using System.Data;
using System.Transactions;
using System.Linq;
using SubSonic;
using VNS.Libs;
using VNS.HIS.DAL;
using System.Text;
using SubSonic;
using NLog;
using VNS.Properties;
using VNS.HIS.NGHIEPVU.THUOC;

namespace VNS.HIS.BusRule.Classes
{
    public class KCB_THANHTOAN
    {
        private NLog.Logger log;
        public KCB_THANHTOAN()
        {
            log = LogManager.GetCurrentClassLogger();
        }
        public DataTable LayDsachBenhnhanThanhtoan(int PatientID, string patient_code, string patientName,
            DateTime fromDate, DateTime toDate, string MaDoituongKcb, int BHYT, string KieuTimKiem, string MAKHOATHIEN)
        {
            return SPs.KcbThanhtoanLaydanhsachBenhnhanThanhtoan(-1,
                   patient_code,
                   patientName,
                   fromDate,
                   toDate,
                  MaDoituongKcb, BHYT,
                   KieuTimKiem, MAKHOATHIEN).GetDataSet().Tables[0];
        }
        public DataTable LaythongtininbienlaiDichvu(int? PaymentID, string MaLuotkham, int? PatientID)
        {
            return SPs.KcbThanhtoanLaythongtinInbienlaiDv(PaymentID, MaLuotkham, PatientID).GetDataSet().Tables[0];
        }
        public DataTable LaythongtininbienlaiBHYT(int? PaymentID, string MaLuotkham, int? PatientID)
        {
            return SPs.KcbThanhtoanLaythongtinInbienlaiBhyt(PaymentID, MaLuotkham, PatientID).GetDataSet().Tables[0];
        }
        public DataTable LaythongtininbienlaiDichvu(KcbThanhtoan objThanhtoan)
        {
            return SPs.KcbThanhtoanLaythongtinInbienlaiDv(objThanhtoan.IdThanhtoan, objThanhtoan.MaLuotkham, objThanhtoan.IdBenhnhan).GetDataSet().Tables[0];
        }
        public DataTable LaythongtininbienlaiBHYT(KcbThanhtoan objThanhtoan)
        {
            return SPs.KcbThanhtoanLaythongtinInbienlaiBhyt(objThanhtoan.IdThanhtoan, objThanhtoan.MaLuotkham, objThanhtoan.IdBenhnhan).GetDataSet().Tables[0];
        }
        public ActionResult HuyThanhtoan(int v_id_thanhtoan, KcbLuotkham ObjPatientExam, string lydohuy, int IdHdonLog, bool HuyBienlai)
        {
            try
            {
                //Kiểm tra trạng thái chốt thanh toán
                KcbThanhtoan _thanhtoan=new Select().From(KcbThanhtoan.Schema).Where
                    (KcbThanhtoan.IdThanhtoanColumn.ColumnName).IsEqualTo(v_id_thanhtoan)
                    .And(KcbThanhtoan.TrangthaiChotColumn.ColumnName).IsEqualTo(1).ExecuteSingle<KcbThanhtoan>();
                if (_thanhtoan != null)
                {
                    Utility.ShowMsg("Thanh toán đang chọn đã được chốt nên bạn không thể hủy thanh toán. Mời bạn xem lại!");
                    return ActionResult.ExistedRecord;//Để ko hiển thị lại thông báo phía client
                }
                DataTable dtKTra = KiemtraTrangthaidonthuocTruockhihuythanhtoan(v_id_thanhtoan);
                if (dtKTra.Rows.Count > 0)
                {
                    Utility.ShowMsg("Lần thanh toán đang chọn có chứa đơn thuốc đã được duyệt cấp phát. Bạn cần liên hệ bộ phận Dược hủy duyệt đơn thuốc trước khi hủy thanh toán");
                    return ActionResult.ExistedRecord;
                }
                if (PropertyLib._ThanhtoanProperties.Hoitruockhihuythanhtoan)
                    if (Utility.AcceptQuestion(string.Format("Bạn có muốn hủy lần thanh toán với Mã thanh toán {0}", v_id_thanhtoan), "Thông báo", true))
                    {
                        return HuyThongTinLanThanhToan(v_id_thanhtoan, ObjPatientExam, lydohuy, IdHdonLog, HuyBienlai);
                    }
                    else
                    {
                        return ActionResult.Cancel;
                    }
                else
                    return HuyThongTinLanThanhToan(v_id_thanhtoan, ObjPatientExam, lydohuy, IdHdonLog, HuyBienlai);
            }
            catch (Exception ex)
            {

                Utility.CatchException(ex);
                return ActionResult.Exception;
            }
            
        }
        public ActionResult HuyThanhtoanDonthuoctaiquay(int v_id_thanhtoan, KcbLuotkham ObjPatientExam, string lydohuy, int IdHdonLog, bool HuyBienlai)
        {
            
            if (PropertyLib._ThanhtoanProperties.Hoitruockhihuythanhtoan)
                if (Utility.AcceptQuestion(string.Format("Bạn có muốn hủy lần thanh toán với Mã thanh toán {0}", v_id_thanhtoan), "Thông báo", true))
                {
                    return HuyThongTinLanThanhToan_Donthuoctaiquay(v_id_thanhtoan, ObjPatientExam, lydohuy, IdHdonLog, HuyBienlai);
                }
                else
                {
                    return ActionResult.Cancel;
                }
            else
                return HuyThongTinLanThanhToan_Donthuoctaiquay(v_id_thanhtoan, ObjPatientExam, lydohuy, IdHdonLog, HuyBienlai);
        }
        public DataTable LaythongtinBenhnhan(string MaLuotkham, int? PatientID)
        {
            return SPs.KcbThanhtoanLaythongtinBenhnhanThanhtoanTheomalankham(MaLuotkham,
                   PatientID).GetDataSet().Tables[0];
        }
        public DataTable LayThongtinChuaThanhtoan(string MaLuotkham, int PatientID, int HosStatus,
            string MAKHOATHIEN, string MADOITUONG)
        {
            return SPs.KcbThanhtoanLaythongtindvuChuathanhtoan(MaLuotkham, PatientID, HosStatus,
                   MAKHOATHIEN, MADOITUONG).
                   GetDataSet().Tables[0];
        }
        public DataTable LayThongtinDaThanhtoan(string MaLuotkham, int PatientID, int HosStatus)
        {
            return SPs.KcbThanhtoanLaythongtindvuDathanhtoan(MaLuotkham, PatientID, HosStatus).
                   GetDataSet().Tables[0];
        }
        public DataTable LayHoaDonCapPhat(string UserName)
        {
            return  SPs.HoadondoLaydanhsachHoadonDacapphatTheouser(UserName).GetDataSet().Tables[0];
        }
        public DataTable LaythongtinCacLanthanhtoan(string MaLuotkham, int? IdBenhnhan, int? KieuThanhToan, string MA_KHOA_THIEN)
        {
            return SPs.KcbThanhtoanLaydanhsachCaclanthanhtoanTheobenhnhan(MaLuotkham,
                       IdBenhnhan, KieuThanhToan,
                       MA_KHOA_THIEN).GetDataSet().Tables[0];
        }
        public DataTable LayPhieuchiThanhtoan(string MaLuotkham, int PatientID, int KieuThanhToan, int Status)
        {
            return SPs.KcbThanhtoanLaythongtinPhieuchiTheobenhnhan(MaLuotkham,
                   PatientID, KieuThanhToan, Status).GetDataSet().Tables[0];
        }
        public DataTable Laythongtinhoadondo(decimal PaymentID)
        {
            return SPs.HoadondoLaythongtinhoadonTheothanhtoan(PaymentID).GetDataSet().Tables[0];
        }
        public DataTable KtraXnhanthuoc(int IdThanhtoan)
        {
            return null;// SPs.DonthuocKiemtraxacnhanthuocTrongdon(IdThanhtoan).GetDataSet().Tables[0];
        }
        public DataTable LaythongtinInphoiBHYT(int PaymentID, string MaLuotkham, int? PatientID, int TuTuc)
        {
            return SPs.BhytLaythongtinInphoi(PaymentID, MaLuotkham,PatientID, TuTuc).GetDataSet().Tables[0];
        }
        private string LayChiKhauChiTiet()
        {
            string PTramChiTiet = "KHONG";
            SqlQuery sqlQuery = new Select().From(SysSystemParameter.Schema)
                .Where(SysSystemParameter.Columns.SName).IsEqualTo("PTRAM_CHITIET");
            SysSystemParameter objSystemParameter = sqlQuery.ExecuteSingle<SysSystemParameter>();
            if (objSystemParameter != null) PTramChiTiet = objSystemParameter.SValue;
            return PTramChiTiet;
        }
        public void XuLyChiKhauDacBietBHYT(KcbLuotkham objLuotkham, decimal ptramBHYT)
        {
            KcbThanhtoanCollection paymentCollection =
                new KcbThanhtoanController().FetchByQuery(
                    KcbThanhtoan.CreateQuery().AddWhere(KcbThanhtoan.Columns.MaLuotkham, Comparison.Equals,
                                                    objLuotkham.MaLuotkham).AND(KcbThanhtoan.Columns.IdBenhnhan,
                                                                                    Comparison.Equals,
                                                                                    objLuotkham.IdBenhnhan));
            foreach (KcbThanhtoan payment in paymentCollection)
            {
                KcbThanhtoanChitietCollection paymentDetailCollection =
                                new KcbThanhtoanChitietController().FetchByQuery(
                                    KcbThanhtoanChitiet.CreateQuery()
                                    .AddWhere(KcbThanhtoanChitiet.Columns.IdThanhtoan,Comparison.Equals, payment.IdThanhtoan)
                                    .AND(KcbThanhtoanChitiet.Columns.TuTuc,Comparison.Equals, 0));
                string IsDungTuyen = "DT";
                    switch (objLuotkham.MaDoituongKcb)
                    {
                        case "BHYT":
                            if (Utility.Int32Dbnull(objLuotkham.DungTuyen, "0") == 1) IsDungTuyen = "DT";
                            else
                            {
                                IsDungTuyen = "TT";
                            }
                            break;
                        default:
                            IsDungTuyen = "KHAC";
                            break;
                    }
                foreach (KcbThanhtoanChitiet PaymentDetail in paymentDetailCollection)
                {
                    SqlQuery sqlQuery = new Select().From(DmucBhytChitraDacbiet.Schema)
                     .Where(DmucBhytChitraDacbiet.Columns.IdDichvuChitiet).IsEqualTo(PaymentDetail.IdChitietdichvu)
                     .And(DmucBhytChitraDacbiet.Columns.MaLoaithanhtoan).IsEqualTo(PaymentDetail.IdLoaithanhtoan)
                     .And(DmucBhytChitraDacbiet.Columns.DungtuyenTraituyen).IsEqualTo(IsDungTuyen)
                     .And(DmucBhytChitraDacbiet.Columns.MaDoituongKcb).IsEqualTo(objLuotkham.MaDoituongKcb);
                    DmucBhytChitraDacbiet objDetailDiscountRate = sqlQuery.ExecuteSingle<DmucBhytChitraDacbiet>();
                    if (objDetailDiscountRate != null)
                    {
                        log.Info("Neu trong ton tai trong bang cau hinh chi tiet chiet khau void Id_Chitiet=" + PaymentDetail.IdChitiet);
                        PaymentDetail.PtramBhyt = objDetailDiscountRate.TileGiam;
                        PaymentDetail.BhytChitra = THU_VIEN_CHUNG.TinhBhytChitra(objDetailDiscountRate.TileGiam,
                                                      Utility.DecimaltoDbnull(
                                                          PaymentDetail.DonGia, 0));
                        PaymentDetail.BnhanChitra = THU_VIEN_CHUNG.TinhBnhanChitra(objDetailDiscountRate.TileGiam,
                                                                 Utility.DecimaltoDbnull(
                                                                     PaymentDetail.DonGia, 0));
                    }
                    else
                    {
                        PaymentDetail.PtramBhyt = ptramBHYT;
                        PaymentDetail.BhytChitra = THU_VIEN_CHUNG.TinhBhytChitra(ptramBHYT,
                                                       Utility.DecimaltoDbnull(
                                                           PaymentDetail.DonGia, 0));
                        PaymentDetail.BnhanChitra = THU_VIEN_CHUNG.TinhBnhanChitra(ptramBHYT,
                                                                 Utility.DecimaltoDbnull(
                                                                     PaymentDetail.DonGia, 0));
                    }
                    log.Info("Thuc hien viec cap nhap thong tin lai gia can phai xem lại gia truoc khi thanh toan");




                }

            }

        }
        private decimal TongtienKhongTutuc(KcbThanhtoanChitiet[] objArrPaymentDetail)
        {
            decimal SumOfPaymentDetail = 0;
            foreach (KcbThanhtoanChitiet paymentDetail in objArrPaymentDetail)
            {
                if (paymentDetail.TuTuc == 0)
                    SumOfPaymentDetail += (Utility.Int32Dbnull(paymentDetail.SoLuong) *
                                           Utility.DecimaltoDbnull(paymentDetail.DonGia));


            }
            return SumOfPaymentDetail;
        }
        public decimal LayThongtinPtramBHYT( decimal v_decTotalMoney, KcbLuotkham objLuotkham, ref  decimal PtramBHYT)
        {
            decimal TIEN_BHYT = 0;
            decimal BHYT_PTRAM_LUONGCOBAN = Utility.DecimaltoDbnull(THU_VIEN_CHUNG.Laygiatrithamsohethong("BHYT_PTRAM_LUONGCOBAN", "0", false), 0);
            SqlQuery q;
            if (THU_VIEN_CHUNG.IsBaoHiem(objLuotkham.IdLoaidoituongKcb.Value))
            {
                ///thực hiện xem có đúng tuyến không

                if (objLuotkham.DungTuyen == 1)
                {
                    //Các đối tượng đặc biệt hưởng 100% BHYT
                    if (Utility.Byte2Bool(objLuotkham.GiayBhyt) || globalVariables.gv_strMaQuyenLoiHuongBHYT100Phantram.Contains(objLuotkham.MaQuyenloi.ToString()))// objLuotkham.MaQuyenloi.ToString() == "1" || objLuotkham.MaQuyenloi.ToString() == "2")
                    {
                        TIEN_BHYT = 0;
                        PtramBHYT = 100;
                        log.Info("Benh nhan tuong ung voi muc =" + objLuotkham.MaQuyenloi);
                    }
                    else
                    {
                        if (BHYT_PTRAM_LUONGCOBAN > 0)
                        {
                            if (v_decTotalMoney >= objLuotkham.LuongCoban * BHYT_PTRAM_LUONGCOBAN / 100)
                            {


                                PtramBHYT = objLuotkham.TrangthaiNoitru <= 0 ? Utility.DecimaltoDbnull(objLuotkham.PtramBhyt, 0) : Utility.DecimaltoDbnull(objLuotkham.PtramBhytGoc, 0);
                                TIEN_BHYT = v_decTotalMoney * (100 - Utility.DecimaltoDbnull(PtramBHYT, 0)) / 100;
                                log.Info("bat dau chi khau theo doi tuong muc tien =" + TIEN_BHYT + " cua benh nhan co ma Patient_Code=" + objLuotkham.MaLuotkham);
                            }
                            else//Tổng tiền < lương cơ bản*% quy định-->BHYT chi trả 100%
                            {

                                PtramBHYT = 100;
                                TIEN_BHYT = 0;
                                log.Info("Benh nhan dc mien phi hoan toan, voi muc chiet khau =0 tuong ung voi Patient_Code=" + objLuotkham.MaLuotkham);
                            }
                        }
                        else
                        {
                            PtramBHYT = Utility.DecimaltoDbnull(objLuotkham.PtramBhyt, 0);
                            TIEN_BHYT = v_decTotalMoney * (100 - Utility.DecimaltoDbnull(objLuotkham.PtramBhyt, 0)) / 100;
                        }

                        #region "cách cũ"
                        //switch (globalVariables.gv_strTuyenBHYT)
                        //{
                        //    case "TUYEN1"://Tuyến huyện. Quan tâm đến lương cơ bản
                        //        if (v_decTotalMoney >= objLuotkham.LuongCoban * globalVariables.gv_intPhantramLuongcoban / 100)
                        //        {


                        //            PtramBHYT = objLuotkham.TrangthaiNoitru <= 0 ? Utility.DecimaltoDbnull(objLuotkham.PtramBhyt, 0) : Utility.DecimaltoDbnull(objLuotkham.PtramBhytGoc, 0);
                        //            TIEN_BHYT = v_decTotalMoney * (100 - Utility.DecimaltoDbnull(PtramBHYT, 0)) / 100;
                        //            log.Info("bat dau chi khau theo doi tuong muc tien =" + TIEN_BHYT + " cua benh nhan co ma Patient_Code=" + objLuotkham.MaLuotkham);
                        //        }
                        //        else//Tổng tiền < lương cơ bản*% quy định-->BHYT chi trả 100%
                        //        {

                        //            PtramBHYT = 100;
                        //            TIEN_BHYT = 0;
                        //            log.Info("Benh nhan dc mien phi hoan toan, voi muc chiet khau =0 tuong ung voi Patient_Code=" + objLuotkham.MaLuotkham);
                        //        }
                        //        break;
                        //    case "TW"://Không quan tâm lương cơ bản
                        //        //Phần cũ
                        //        //q = new Select().From(DmucDoituongbhyt.Schema)
                        //        //   .Where(DmucDoituongbhyt.Columns.IdDoituongKcb).IsEqualTo(objLuotkham.IdDoituongKcb)
                        //        //   .And(DmucDoituongbhyt.Columns.MaDoituongbhyt).IsEqualTo(objLuotkham.MaDoituongBhyt);
                        //        //DmucDoituongbhyt objInsuranceObjectTW = q.ExecuteSingle<DmucDoituongbhyt>();
                        //        //if (objInsuranceObjectTW != null)
                        //        //{
                        //        //    PtramBHYT = Utility.DecimaltoDbnull(objInsuranceObjectTW.PhantramBhyt, 0);
                        //        //    TIEN_BHYT = v_decTotalMoney * (100 - Utility.DecimaltoDbnull(objInsuranceObjectTW.PhantramBhyt, 0)) / 100;
                        //        //    log.Info("bat dau chi khau theo doi tuong muc tien =" + TIEN_BHYT + " cua benh nhan co ma Patient_Code=" + objLuotkham.MaLuotkham);
                        //        //}
                        //        PtramBHYT = Utility.DecimaltoDbnull(objLuotkham.PtramBhyt, 0);
                        //        TIEN_BHYT = v_decTotalMoney * (100 - Utility.DecimaltoDbnull(objLuotkham.PtramBhyt, 0)) / 100;

                        //        break;
                        //    default://Khác
                        //        if (v_decTotalMoney >= objLuotkham.LuongCoban * globalVariables.gv_intPhantramLuongcoban / 100)
                        //        {
                        //            PtramBHYT = Utility.DecimaltoDbnull(objLuotkham.PtramBhyt, 0);
                        //            TIEN_BHYT = v_decTotalMoney * (100 - Utility.DecimaltoDbnull(PtramBHYT, 0)) / 100;
                        //        }
                        //        else
                        //        {

                        //            PtramBHYT = 100;
                        //            TIEN_BHYT = 0;
                        //            log.Info("Benh nhan dc mien phi hoan toan, voi muc chiet khau =0 tuong ung voi Patient_Code=" + objLuotkham.MaLuotkham);
                        //        }
                        //        break;
                        //}
                        #endregion
                       
                    }
                }
                else//Trái tuyến
                {
                    ///Nếu là đối tượng trái tuyến thực hiện lấy % của trái tuyến
                    //DmucDoituongkcb objObjectType = DmucDoituongkcb.FetchByID(objLuotkham.IdDoituongKcb);
                    //if (objObjectType != null)
                    //{
                    TIEN_BHYT = v_decTotalMoney * (100 - Utility.DecimaltoDbnull(objLuotkham.PtramBhyt)) / 100;
                    PtramBHYT = objLuotkham.TrangthaiNoitru <= 0 ? Utility.DecimaltoDbnull(objLuotkham.PtramBhyt, 0) : Utility.DecimaltoDbnull(objLuotkham.PtramBhytGoc, 0);
                    //}
                }
            }
            else//PtramBhyt=0
            {
                //DmucDoituongkcb objObjectType = DmucDoituongkcb.FetchByID(objLuotkham.IdDoituongKcb);
                //if (objObjectType != null)
                TIEN_BHYT = v_decTotalMoney * (100 - Utility.Int32Dbnull(objLuotkham.PtramBhyt, 0)) / 100; ;
                PtramBHYT = Utility.DecimaltoDbnull(objLuotkham.PtramBhyt, 0);
            }
            return TIEN_BHYT;
        }
        public ActionResult ThanhtoanDonthuoctaiquay(KcbThanhtoan objThanhtoan, KcbDanhsachBenhnhan objBenhnhan, KcbThanhtoanChitiet[] objArrPaymentDetail, ref int id_thanhtoan, long IdHdonLog, bool Layhoadondo)
        {

            decimal PtramBHYT = 0;
            ///tổng tiền hiện tại truyền vào của lần payment đang thực hiện
            decimal v_TotalOrginPrice = 0;
            ///tổng tiền đã thanh toán
            decimal v_TotalPaymentDetail = 0;
            try
            {
                using (var scope = new TransactionScope())
                {
                    using (var dbscope = new SharedDbConnectionScope())
                    {
                        ///lấy tổng số Payment của mang truyền vào của pay ment hiện tại
                        v_TotalOrginPrice = TongtienKhongTutuc(objArrPaymentDetail);
                        KcbThanhtoanCollection paymentCollection =
                            new KcbThanhtoanController()
                            .FetchByQuery(
                                KcbThanhtoan.CreateQuery()
                                .AddWhere
                                //(KcbThanhtoan.Columns.MaLuotkham, Comparison.Equals, objLuotkham.MaLuotkham).AND
                                (KcbThanhtoan.Columns.IdBenhnhan, Comparison.Equals, objBenhnhan.IdBenhnhan)
                                .AND(KcbThanhtoan.Columns.TrangThai, Comparison.Equals, 0)
                                .AND(KcbThanhtoan.Columns.KieuThanhtoan, Comparison.Equals, 0)
                                .AND(KcbThanhtoan.Columns.TrangThai, Comparison.Equals, 0));
                        //Lấy tổng tiền của các lần thanh toán trước
                        int id_donthuoc = -1;
                        foreach (KcbThanhtoan Payment in paymentCollection)
                        {
                            KcbThanhtoanChitietCollection paymentDetailCollection = new Select().From(KcbThanhtoanChitiet.Schema)
                                .Where(KcbThanhtoanChitiet.Columns.IdThanhtoan).IsEqualTo(Payment.IdThanhtoan)
                                .And(KcbThanhtoanChitiet.Columns.TrangthaiHuy).IsEqualTo(0).ExecuteAsCollection
                                <KcbThanhtoanChitietCollection>();

                            foreach (KcbThanhtoanChitiet paymentDetail in paymentDetailCollection)
                            {
                                if (id_donthuoc == -1) id_donthuoc = paymentDetail.IdPhieu;
                                if (paymentDetail.TuTuc == 0)
                                    v_TotalPaymentDetail += Utility.Int32Dbnull(paymentDetail.SoLuong) *
                                                            Utility.DecimaltoDbnull(paymentDetail.DonGia);

                            }
                        }
                      
                        //LayThongtinPtramBHYT(v_TotalOrginPrice + v_TotalPaymentDetail, objLuotkham, ref PtramBHYT);
                        objThanhtoan.MaThanhtoan = THU_VIEN_CHUNG.TaoMathanhtoan(Convert.ToDateTime(objThanhtoan.NgayThanhtoan));
                        objThanhtoan.IsNew = true;
                        objThanhtoan.Save();
                        if (id_donthuoc == -1) id_donthuoc = objArrPaymentDetail[0].IdPhieu;
                        KcbDonthuoc objDonthuoc = KcbDonthuoc.FetchByID(id_donthuoc);
                        KcbDonthuocChitietCollection lstChitiet = new Select().From(KcbDonthuoc.Schema).Where(KcbDonthuoc.Columns.IdDonthuoc).IsEqualTo(id_donthuoc).ExecuteAsCollection<KcbDonthuocChitietCollection>();
                        ActionResult actionResult = ActionResult.Success;
                        if (objDonthuoc != null && lstChitiet.Count>0)
                        {
                            if (!XuatThuoc.InValiKiemTraDonThuoc(lstChitiet,(byte)0)) return ActionResult.NotEnoughDrugInStock;
                            actionResult = new XuatThuoc().LinhThuocBenhNhan(id_donthuoc, Utility.Int16Dbnull(lstChitiet[0].IdKho, 0), globalVariables.SysDate);
                            switch (actionResult)
                            {
                                case ActionResult.Success:
                                  
                                    break;
                                case ActionResult.Error:
                                    return actionResult;
                            }
                        }
                        //Tính lại Bnhan chi trả và BHYT chi trả
                        //objArrPaymentDetail = THU_VIEN_CHUNG.TinhPhamTramBHYT(objArrPaymentDetail, PtramBHYT);
                        decimal TT_BN = 0m;
                        decimal TT_BHYT = 0m;
                        decimal TT_Chietkhau_Chitiet = 0m;
                        foreach (KcbThanhtoanChitiet objThanhtoanDetail in objArrPaymentDetail)
                        {
                            TT_BN += (objThanhtoanDetail.BnhanChitra + objThanhtoanDetail.PhuThu) * objThanhtoanDetail.SoLuong;
                            TT_BHYT += objThanhtoanDetail.BhytChitra * objThanhtoanDetail.SoLuong;
                            TT_Chietkhau_Chitiet += Utility.DecimaltoDbnull(objThanhtoanDetail.TienChietkhau, 0);
                            objThanhtoanDetail.IdThanhtoan = Utility.Int32Dbnull(objThanhtoan.IdThanhtoan, -1);
                            objThanhtoanDetail.IsNew = true;
                            objThanhtoanDetail.Save();
                            UpdatePaymentStatus(objThanhtoan, objThanhtoanDetail);
                        }

                        #region Hoadondo

                        if (Layhoadondo)
                        {
                            int record = -1;
                            if (IdHdonLog > 0)
                            {
                                record =
                                    new Delete().From(HoadonLog.Schema)
                                        .Where(HoadonLog.Columns.IdHdonLog)
                                        .IsEqualTo(IdHdonLog)
                                        .Execute();
                                if (record <= 0)
                                {
                                    Utility.ShowMsg("Có lỗi trong quá trình xóa thông tin serie hóa đơn đã hủy để cấp lại cho lần thanh toán này.");
                                    return ActionResult.Error;
                                }
                            }
                            var obj = new HoadonLog();
                            obj.IdThanhtoan = objThanhtoan.IdThanhtoan;
                            obj.TongTien = objThanhtoan.TongTien - Utility.DecimaltoDbnull(objThanhtoan.TongtienChietkhau, 0);
                            obj.IdBenhnhan = objThanhtoan.IdBenhnhan;
                            obj.MaLuotkham = objThanhtoan.MaLuotkham;
                            obj.MauHoadon = objThanhtoan.MauHoadon;
                            obj.KiHieu = objThanhtoan.KiHieu;
                            obj.IdCapphat = objThanhtoan.IdCapphat.Value;
                            obj.MaQuyen = objThanhtoan.MaQuyen;
                            obj.Serie = objThanhtoan.Serie;
                            obj.MaNhanvien = globalVariables.UserName;
                            obj.MaLydo = "0";
                            obj.NgayIn = globalVariables.SysDate;
                            obj.TrangThai = 0;
                            obj.IsNew = true;
                            obj.Save();
                            IdHdonLog = obj.IdHdonLog;//Để update lại vào bảng thanh toán
                            new Update(HoadonCapphat.Schema).Set(HoadonCapphat.Columns.SerieHientai)
                                .EqualTo(objThanhtoan.Serie)
                                .Set(HoadonCapphat.Columns.TrangThai).EqualTo(1)
                                .Where(HoadonCapphat.Columns.IdCapphat).IsEqualTo(obj.IdCapphat)
                                .Execute();
                        }
                        #endregion
                        KcbPhieuthu objPhieuthu = new KcbPhieuthu();
                        objPhieuthu.IdThanhtoan = objThanhtoan.IdThanhtoan;
                        objPhieuthu.MaPhieuthu = THU_VIEN_CHUNG.GetMaPhieuThu(globalVariables.SysDate, 0);
                        objPhieuthu.SoluongChungtugoc = 1;
                        objPhieuthu.LoaiPhieuthu = Convert.ToByte(0);
                        objPhieuthu.NgayThuchien = globalVariables.SysDate;
                        objPhieuthu.SoTien = TT_BN - TT_Chietkhau_Chitiet;
                        objPhieuthu.SotienGoc = TT_BN;
                        objPhieuthu.MaLydoChietkhau = objThanhtoan.MaLydoChietkhau;
                        objPhieuthu.TienChietkhauchitiet = TT_Chietkhau_Chitiet;
                        objPhieuthu.TienChietkhau = objThanhtoan.TongtienChietkhau;
                        objPhieuthu.TienChietkhauhoadon = objPhieuthu.TienChietkhau - objPhieuthu.TienChietkhauchitiet;
                        objPhieuthu.NguoiNop = globalVariables.UserName;
                        objPhieuthu.TaikhoanCo = "";
                        objPhieuthu.TaikhoanNo = "";
                        objPhieuthu.LydoNop = "Thu tiền bệnh nhân";
                        objPhieuthu.IdKhoaThuchien = globalVariables.idKhoatheoMay;
                        objPhieuthu.IdNhanvien = globalVariables.gv_intIDNhanvien;
                        objPhieuthu.IsNew = true;
                        objPhieuthu.Save();

                        new Update(KcbThanhtoan.Schema)
                        .Set(KcbThanhtoan.Columns.TongTien).EqualTo(TT_BHYT + TT_BN)
                        .Set(KcbThanhtoan.Columns.BnhanChitra).EqualTo(TT_BN)
                        .Set(KcbThanhtoan.Columns.BhytChitra).EqualTo(TT_BHYT)
                        .Set(KcbThanhtoan.Columns.IdHdonLog).EqualTo(IdHdonLog)
                        .Where(KcbThanhtoan.Columns.IdThanhtoan).IsEqualTo(objThanhtoan.IdThanhtoan).Execute();
                    }
                    scope.Complete();
                    id_thanhtoan = Utility.Int32Dbnull(objThanhtoan.IdThanhtoan, -1);
                    return ActionResult.Success;
                }
            }
            catch (Exception ex)
            {
                log.Error("Loi thuc hien thanh toan:" + ex.ToString());
                return ActionResult.Error;
            }

        }

        public ActionResult Payment4SelectedItems(KcbThanhtoan objThanhtoan, KcbLuotkham objLuotkham, KcbThanhtoanChitiet[] objArrPaymentDetail, ref int id_thanhtoan, long IdHdonLog,bool Layhoadondo)
        {

            decimal PtramBHYT = 0;
            ///tổng tiền hiện tại truyền vào của lần payment đang thực hiện
            decimal v_TotalOrginPrice = 0;
            ///tổng tiền đã thanh toán
            decimal v_TotalPaymentDetail = 0;
            try
            {
                using (var scope = new TransactionScope())
                {
                    using (var dbscope = new SharedDbConnectionScope())
                    {
                        ///lấy tổng số Payment của mang truyền vào của pay ment hiện tại
                        v_TotalOrginPrice = TongtienKhongTutuc(objArrPaymentDetail);
                        KcbThanhtoanCollection paymentCollection =
                            new KcbThanhtoanController()
                            .FetchByQuery(
                                KcbThanhtoan.CreateQuery()
                                .AddWhere(KcbThanhtoan.Columns.MaLuotkham, Comparison.Equals,objLuotkham.MaLuotkham)
                                .AND(KcbThanhtoan.Columns.IdBenhnhan, Comparison.Equals,objLuotkham.IdBenhnhan)
                                .AND(KcbThanhtoan.Columns.TrangThai, Comparison.Equals, 0)
                                .AND(KcbThanhtoan.Columns.KieuThanhtoan, Comparison.Equals, 0)
                                .AND(KcbThanhtoan.Columns.TrangThai, Comparison.Equals, 0));
                        //Lấy tổng tiền của các lần thanh toán trước
                        foreach (KcbThanhtoan Payment in paymentCollection)
                        {
                            KcbThanhtoanChitietCollection paymentDetailCollection = new Select().From(KcbThanhtoanChitiet.Schema)
                                .Where(KcbThanhtoanChitiet.Columns.IdThanhtoan).IsEqualTo(Payment.IdThanhtoan)
                                .And(KcbThanhtoanChitiet.Columns.TrangthaiHuy).IsEqualTo(0).ExecuteAsCollection
                                <KcbThanhtoanChitietCollection>();

                            foreach (KcbThanhtoanChitiet paymentDetail in paymentDetailCollection)
                            {
                                if (paymentDetail.TuTuc == 0)
                                    v_TotalPaymentDetail += Utility.Int32Dbnull(paymentDetail.SoLuong) *
                                                            Utility.DecimaltoDbnull(paymentDetail.DonGia);

                            }
                        }
                        //Tính toán lại phần trăm BHYT chủ yếu liên quan đến phần lương cơ bản. 
                        //Phần trăm này có thể bị biến đổi và khác với % trong bảng lượt khám
                        LayThongtinPtramBHYT(v_TotalOrginPrice + v_TotalPaymentDetail, objLuotkham, ref PtramBHYT);
                        objThanhtoan.MaThanhtoan = THU_VIEN_CHUNG.TaoMathanhtoan(Convert.ToDateTime(objThanhtoan.NgayThanhtoan));
                        objThanhtoan.IsNew = true;
                        objThanhtoan.Save();
                       //Tính lại Bnhan chi trả và BHYT chi trả
                        objArrPaymentDetail = THU_VIEN_CHUNG.TinhPhamTramBHYT(objLuotkham, objArrPaymentDetail, PtramBHYT);
                        decimal TT_BN = 0m;
                        decimal TT_BHYT = 0m;
                        decimal TT_Chietkhau_Chitiet = 0m;
                        foreach (KcbThanhtoanChitiet objThanhtoanDetail in objArrPaymentDetail)
                        {
                            TT_BN += (objThanhtoanDetail.BnhanChitra + objThanhtoanDetail.PhuThu) * objThanhtoanDetail.SoLuong;
                            TT_BHYT += objThanhtoanDetail.BhytChitra * objThanhtoanDetail.SoLuong;
                            TT_Chietkhau_Chitiet += Utility.DecimaltoDbnull(objThanhtoanDetail.TienChietkhau, 0);
                            objThanhtoanDetail.IdThanhtoan = Utility.Int32Dbnull(objThanhtoan.IdThanhtoan, -1);
                            objThanhtoanDetail.IsNew = true;
                            objThanhtoanDetail.Save();
                            UpdatePaymentStatus(objThanhtoan, objThanhtoanDetail);
                        }

                        #region Hoadondo
                       
                        if (Layhoadondo)
                        {
                            int record = -1;
                            if (IdHdonLog > 0)
                            {
                                record =
                                    new Delete().From(HoadonLog.Schema)
                                        .Where(HoadonLog.Columns.IdHdonLog)
                                        .IsEqualTo(IdHdonLog)
                                        .Execute();
                                if (record <= 0)
                                {
                                    Utility.ShowMsg("Có lỗi trong quá trình xóa thông tin serie hóa đơn đã hủy để cấp lại cho lần thanh toán này.");
                                    return ActionResult.Error;
                                }
                            }
                            var obj = new HoadonLog();
                            obj.IdThanhtoan = objThanhtoan.IdThanhtoan;
                            obj.TongTien = objThanhtoan.TongTien - Utility.DecimaltoDbnull(objThanhtoan.TongtienChietkhau, 0);
                            obj.IdBenhnhan = objThanhtoan.IdBenhnhan;
                            obj.MaLuotkham = objThanhtoan.MaLuotkham;
                            obj.MauHoadon = objThanhtoan.MauHoadon;
                            obj.KiHieu = objThanhtoan.KiHieu;
                            obj.IdCapphat = objThanhtoan.IdCapphat.Value;
                            obj.MaQuyen = objThanhtoan.MaQuyen;
                            obj.Serie = objThanhtoan.Serie;
                            obj.MaNhanvien = globalVariables.UserName;
                            obj.MaLydo = "0";
                            obj.NgayIn = globalVariables.SysDate;
                            obj.TrangThai = 0;
                            obj.IsNew = true;
                            obj.Save();
                            IdHdonLog = obj.IdHdonLog;//Để update lại vào bảng thanh toán
                            new Update(HoadonCapphat.Schema).Set(HoadonCapphat.Columns.SerieHientai)
                                .EqualTo(objThanhtoan.Serie)
                                .Set(HoadonCapphat.Columns.TrangThai).EqualTo(1)
                                .Where(HoadonCapphat.Columns.IdCapphat).IsEqualTo(obj.IdCapphat)
                                .Execute();
                        }
                        #endregion

                        KcbPhieuthu objPhieuthu = new KcbPhieuthu();
                        objPhieuthu.IdThanhtoan = objThanhtoan.IdThanhtoan;
                        objPhieuthu.MaPhieuthu = THU_VIEN_CHUNG.GetMaPhieuThu(globalVariables.SysDate, 0);
                        objPhieuthu.SoluongChungtugoc = 1;
                        objPhieuthu.LoaiPhieuthu = Convert.ToByte(0);
                        objPhieuthu.NgayThuchien = globalVariables.SysDate;
                        objPhieuthu.SoTien = TT_BN - TT_Chietkhau_Chitiet;
                        objPhieuthu.SotienGoc = TT_BN;
                        objPhieuthu.MaLydoChietkhau = objThanhtoan.MaLydoChietkhau;
                        objPhieuthu.TienChietkhauchitiet = TT_Chietkhau_Chitiet;
                        objPhieuthu.TienChietkhau = objThanhtoan.TongtienChietkhau;
                        objPhieuthu.TienChietkhauhoadon = objPhieuthu.TienChietkhau - objPhieuthu.TienChietkhauchitiet;
                        objPhieuthu.NguoiNop = globalVariables.UserName;
                        objPhieuthu.TaikhoanCo = "";
                        objPhieuthu.TaikhoanNo = "";
                        objPhieuthu.LydoNop = "Thu tiền bệnh nhân";
                        objPhieuthu.IdKhoaThuchien = globalVariables.idKhoatheoMay;
                        objPhieuthu.IdNhanvien = globalVariables.gv_intIDNhanvien;
                        objPhieuthu.IsNew = true;
                        objPhieuthu.Save();

                        new Update(KcbThanhtoan.Schema)
                        .Set(KcbThanhtoan.Columns.TongTien).EqualTo(TT_BHYT + TT_BN)
                        .Set(KcbThanhtoan.Columns.BnhanChitra).EqualTo(TT_BN)
                        .Set(KcbThanhtoan.Columns.BhytChitra).EqualTo(TT_BHYT)
                        .Set(KcbThanhtoan.Columns.MaDoituongKcb).EqualTo(objLuotkham.MaDoituongKcb)
                        .Set(KcbThanhtoan.Columns.IdDoituongKcb).EqualTo(objLuotkham.IdDoituongKcb)
                        .Set(KcbThanhtoan.Columns.PtramBhyt).EqualTo(objLuotkham.PtramBhyt)
                        .Set(KcbThanhtoan.Columns.IdHdonLog).EqualTo(IdHdonLog)
                        .Where(KcbThanhtoan.Columns.IdThanhtoan).IsEqualTo(objThanhtoan.IdThanhtoan).Execute();
                    }
                    scope.Complete();
                    id_thanhtoan = Utility.Int32Dbnull(objThanhtoan.IdThanhtoan, -1);
                    return ActionResult.Success;
                }
            }
            catch (Exception ex)
            {
                log.Error("Loi thuc hien thanh toan:" + ex.ToString());
                return ActionResult.Error;
            }

        }
        public ActionResult Payment4SelectedItems_Ao(KcbThanhtoan objThanhtoan, KcbLuotkham objLuotkham, KcbThanhtoanChitiet[] objArrPaymentDetail, ref int id_thanhtoan, long IdHdonLog, bool Layhoadondo)
        {

            decimal PtramBHYT = 0;
            ///tổng tiền hiện tại truyền vào của lần payment đang thực hiện
            decimal v_TotalOrginPrice = 0;
            ///tổng tiền đã thanh toán
            decimal v_TotalPaymentDetail = 0;
            try
            {
                using (var scope = new TransactionScope())
                {
                    using (var dbscope = new SharedDbConnectionScope())
                    {
                        ///lấy tổng số Payment của mang truyền vào của pay ment hiện tại
                        v_TotalOrginPrice = TongtienKhongTutuc(objArrPaymentDetail);
                        KcbThanhtoanCollection paymentCollection =
                            new KcbThanhtoanController()
                            .FetchByQuery(
                                KcbThanhtoan.CreateQuery()
                                .AddWhere(KcbThanhtoan.Columns.MaLuotkham, Comparison.Equals, objLuotkham.MaLuotkham)
                                .AND(KcbThanhtoan.Columns.IdBenhnhan, Comparison.Equals, objLuotkham.IdBenhnhan)
                                .AND(KcbThanhtoan.Columns.TrangThai, Comparison.Equals, 0)
                                .AND(KcbThanhtoan.Columns.KieuThanhtoan, Comparison.Equals, 0)
                                .AND(KcbThanhtoan.Columns.TrangThai, Comparison.Equals, 0));
                        //Lấy tổng tiền của các lần thanh toán trước
                        foreach (KcbThanhtoan Payment in paymentCollection)
                        {
                            KcbThanhtoanChitietCollection paymentDetailCollection = new Select().From(KcbThanhtoanChitiet.Schema)
                                .Where(KcbThanhtoanChitiet.Columns.IdThanhtoan).IsEqualTo(Payment.IdThanhtoan)
                                .And(KcbThanhtoanChitiet.Columns.TrangthaiHuy).IsEqualTo(0).ExecuteAsCollection
                                <KcbThanhtoanChitietCollection>();

                            foreach (KcbThanhtoanChitiet paymentDetail in paymentDetailCollection)
                            {
                                if (paymentDetail.TuTuc == 0)
                                    v_TotalPaymentDetail += Utility.Int32Dbnull(paymentDetail.SoLuong) *
                                                            Utility.DecimaltoDbnull(paymentDetail.DonGia);

                            }
                        }
                        //Tính toán lại phần trăm BHYT chủ yếu liên quan đến phần lương cơ bản. 
                        //Phần trăm này có thể bị biến đổi và khác với % trong bảng lượt khám
                        LayThongtinPtramBHYT(v_TotalOrginPrice + v_TotalPaymentDetail, objLuotkham, ref PtramBHYT);
                        objThanhtoan.MaThanhtoan = THU_VIEN_CHUNG.TaoMathanhtoan(Convert.ToDateTime(objThanhtoan.NgayThanhtoan));
                        objThanhtoan.IsNew = true;
                        objThanhtoan.Save();
                        //Tính lại Bnhan chi trả và BHYT chi trả
                        objArrPaymentDetail = THU_VIEN_CHUNG.TinhPhamTramBHYT(objLuotkham, objArrPaymentDetail, PtramBHYT);
                        decimal TT_BN = 0m;
                        decimal TT_BHYT = 0m;
                        decimal TT_Chietkhau_Chitiet = 0m;
                        foreach (KcbThanhtoanChitiet objThanhtoanDetail in objArrPaymentDetail)
                        {
                            TT_BN += (objThanhtoanDetail.BnhanChitra + objThanhtoanDetail.PhuThu) * objThanhtoanDetail.SoLuong;
                            TT_BHYT += objThanhtoanDetail.BhytChitra * objThanhtoanDetail.SoLuong;
                            TT_Chietkhau_Chitiet += Utility.DecimaltoDbnull(objThanhtoanDetail.TienChietkhau, 0);
                            objThanhtoanDetail.IdThanhtoan = Utility.Int32Dbnull(objThanhtoan.IdThanhtoan, -1);
                            objThanhtoanDetail.IsNew = true;
                            objThanhtoanDetail.Save();
                            UpdatePaymentStatus(objThanhtoan, objThanhtoanDetail);
                        }

                        #region Hoadondo

                        if (Layhoadondo)
                        {
                            int record = -1;
                            if (IdHdonLog > 0)
                            {
                                record =
                                    new Delete().From(HoadonLog.Schema)
                                        .Where(HoadonLog.Columns.IdHdonLog)
                                        .IsEqualTo(IdHdonLog)
                                        .Execute();
                                if (record <= 0)
                                {
                                    Utility.ShowMsg("Có lỗi trong quá trình xóa thông tin serie hóa đơn đã hủy để cấp lại cho lần thanh toán này.");
                                    return ActionResult.Error;
                                }
                            }
                            var obj = new HoadonLog();
                            obj.IdThanhtoan = objThanhtoan.IdThanhtoan;
                            obj.TongTien = objThanhtoan.TongTien - Utility.DecimaltoDbnull(objThanhtoan.TongtienChietkhau, 0);
                            obj.IdBenhnhan = objThanhtoan.IdBenhnhan;
                            obj.MaLuotkham = objThanhtoan.MaLuotkham;
                            obj.MauHoadon = objThanhtoan.MauHoadon;
                            obj.KiHieu = objThanhtoan.KiHieu;
                            obj.IdCapphat = objThanhtoan.IdCapphat.Value;
                            obj.MaQuyen = objThanhtoan.MaQuyen;
                            obj.Serie = objThanhtoan.Serie;
                            obj.MaNhanvien = globalVariables.UserName;
                            obj.MaLydo = "0";
                            obj.NgayIn = globalVariables.SysDate;
                            obj.TrangThai = 0;
                            obj.IsNew = true;
                            obj.Save();
                            IdHdonLog = obj.IdHdonLog;//Để update lại vào bảng thanh toán
                            new Update(HoadonCapphat.Schema).Set(HoadonCapphat.Columns.SerieHientai)
                                .EqualTo(objThanhtoan.Serie)
                                .Set(HoadonCapphat.Columns.TrangThai).EqualTo(1)
                                .Where(HoadonCapphat.Columns.IdCapphat).IsEqualTo(obj.IdCapphat)
                                .Execute();
                        }
                        #endregion

                        KcbPhieuthu objPhieuthu = new KcbPhieuthu();
                        objPhieuthu.IdThanhtoan = objThanhtoan.IdThanhtoan;
                        objPhieuthu.MaPhieuthu = THU_VIEN_CHUNG.GetMaPhieuThu(globalVariables.SysDate, 0);
                        objPhieuthu.SoluongChungtugoc = 1;
                        objPhieuthu.LoaiPhieuthu = Convert.ToByte(0);
                        objPhieuthu.NgayThuchien = globalVariables.SysDate;
                        objPhieuthu.SoTien = TT_BN - TT_Chietkhau_Chitiet;
                        objPhieuthu.SotienGoc = TT_BN;
                        objPhieuthu.MaLydoChietkhau = objThanhtoan.MaLydoChietkhau;
                        objPhieuthu.TienChietkhauchitiet = TT_Chietkhau_Chitiet;
                        objPhieuthu.TienChietkhau = objThanhtoan.TongtienChietkhau;
                        objPhieuthu.TienChietkhauhoadon = objPhieuthu.TienChietkhau - objPhieuthu.TienChietkhauchitiet;
                        objPhieuthu.NguoiNop = globalVariables.UserName;
                        objPhieuthu.TaikhoanCo = "";
                        objPhieuthu.TaikhoanNo = "";
                        objPhieuthu.LydoNop = "Thu tiền bệnh nhân";
                        objPhieuthu.IdKhoaThuchien = globalVariables.idKhoatheoMay;
                        objPhieuthu.IdNhanvien = globalVariables.gv_intIDNhanvien;
                        objPhieuthu.IsNew = true;
                        objPhieuthu.Save();

                        objLuotkham.IsNew = false;
                        objLuotkham.MarkOld();
                        objLuotkham.Save();

                        new Update(KcbThanhtoan.Schema)
                        .Set(KcbThanhtoan.Columns.TongTien).EqualTo(TT_BHYT + TT_BN)
                        .Set(KcbThanhtoan.Columns.BnhanChitra).EqualTo(TT_BN)
                        .Set(KcbThanhtoan.Columns.BhytChitra).EqualTo(TT_BHYT)
                        .Set(KcbThanhtoan.Columns.MaDoituongKcb).EqualTo(objLuotkham.MaDoituongKcb)
                        .Set(KcbThanhtoan.Columns.IdDoituongKcb).EqualTo(objLuotkham.IdDoituongKcb)
                        .Set(KcbThanhtoan.Columns.PtramBhyt).EqualTo(objLuotkham.PtramBhyt)
                        .Set(KcbThanhtoan.Columns.IdHdonLog).EqualTo(IdHdonLog)
                        .Where(KcbThanhtoan.Columns.IdThanhtoan).IsEqualTo(objThanhtoan.IdThanhtoan).Execute();
                    }
                    scope.Complete();
                    id_thanhtoan = Utility.Int32Dbnull(objThanhtoan.IdThanhtoan, -1);
                    return ActionResult.Success;
                }
            }
            catch (Exception ex)
            {
                log.Error("Loi thuc hien thanh toan:" + ex.ToString());
                return ActionResult.Error;
            }

        }
        public ActionResult LayHoadondo(long id_thanhtoan, string MauHoadon,string KiHieu,string MaQuyen,string Serie,int IdCapphat, long IdHdonLog_huy,ref long IdHdonLog)
        {
            try
            {
                using (var scope = new TransactionScope())
                {
                    using (var db = new SharedDbConnectionScope())
                    {
                        KcbThanhtoan objThanhtoan = KcbThanhtoan.FetchByID(id_thanhtoan);
                        if (objThanhtoan == null) return ActionResult.Error;

                        if (IdHdonLog_huy > 0)
                        {
                           int record =
                                new Delete().From(HoadonLog.Schema)
                                    .Where(HoadonLog.Columns.IdHdonLog)
                                    .IsEqualTo(IdHdonLog_huy)
                                    .Execute();
                        }

                        var obj = new HoadonLog();
                        obj.IdThanhtoan = objThanhtoan.IdThanhtoan;
                        obj.TongTien = objThanhtoan.TongTien - Utility.DecimaltoDbnull(objThanhtoan.TongtienChietkhau, 0);
                        obj.IdBenhnhan = objThanhtoan.IdBenhnhan;
                        obj.MaLuotkham = objThanhtoan.MaLuotkham;
                        obj.MauHoadon = MauHoadon;
                        obj.KiHieu = KiHieu;
                        obj.IdCapphat = IdCapphat;
                        obj.MaQuyen = MaQuyen;
                        obj.Serie = Serie;
                        obj.MaNhanvien = globalVariables.UserName;
                        obj.MaLydo = "0";
                        obj.NgayIn = globalVariables.SysDate;
                        obj.TrangThai = 0;
                        obj.IsNew = true;
                        obj.Save();
                        IdHdonLog = obj.IdHdonLog;
                        new Update(KcbThanhtoan.Schema)
                        .Set(KcbThanhtoan.Columns.Serie).EqualTo(Serie)
                        .Set(KcbThanhtoan.Columns.MauHoadon).EqualTo(MauHoadon)
                        .Set(KcbThanhtoan.Columns.MaQuyen).EqualTo(MaQuyen)
                        .Set(KcbThanhtoan.Columns.KiHieu).EqualTo(KiHieu)
                        .Set(KcbThanhtoan.Columns.IdHdonLog).EqualTo(obj.IdHdonLog)
                        .Set(KcbThanhtoan.Columns.IdCapphat).EqualTo(obj.IdCapphat)
                        .Set(KcbThanhtoan.Columns.TrangthaiSeri).EqualTo(0)
                       .Where(KcbThanhtoan.Columns.IdThanhtoan).IsEqualTo(id_thanhtoan).Execute();
                    }
                    scope.Complete();
                    return ActionResult.Success;
                }
            }
            catch
            {
                return ActionResult.Exception;
            }
        }
        public ActionResult BoHoadondo( long IdHdonLog)
        {
            try
            {
                using (var scope = new TransactionScope())
                {
                    using (var db = new SharedDbConnectionScope())
                    {
                        new Delete().From(HoadonLog.Schema)
                       .Where(HoadonLog.Columns.IdHdonLog).IsEqualTo(IdHdonLog).Execute();
                        new Update(KcbThanhtoan.Schema)
                        .Set(KcbThanhtoan.Columns.Serie).EqualTo("")
                        .Set(KcbThanhtoan.Columns.MauHoadon).EqualTo("")
                        .Set(KcbThanhtoan.Columns.MaQuyen).EqualTo("")
                        .Set(KcbThanhtoan.Columns.KiHieu).EqualTo("")
                        .Set(KcbThanhtoan.Columns.IdHdonLog).EqualTo(-1)
                        .Set(KcbThanhtoan.Columns.IdCapphat).EqualTo(-1)
                        .Set(KcbThanhtoan.Columns.TrangthaiSeri).EqualTo(0)
                       .Where(KcbThanhtoan.Columns.IdHdonLog).IsEqualTo(IdHdonLog).Execute();
                    }
                    scope.Complete();
                    return ActionResult.Success;
                }
            }
            catch
            {
                return ActionResult.Exception;
            }
        }

         public ActionResult UpdatePtramBHYT(KcbLuotkham objLuotKham, int option)
        {
            try
            {
                 using (var scope = new TransactionScope())
                {
                    using (var db = new SharedDbConnectionScope())
                    {
                        decimal ptramBhyt = Utility.DecimaltoDbnull(objLuotKham.PtramBhyt, 0m);
                        decimal bnhanchitra = 0m;
                        decimal bhytchitra = 0m;
                        decimal dongia=0m;
                        if (option == 1 || option == -1)
                        {
                            KcbDangkyKcbCollection lstKcbDangkyKcb = new Select().From(KcbDangkyKcb.Schema)
                                .Where(KcbDangkyKcb.Columns.MaLuotkham).IsEqualTo(objLuotKham.MaLuotkham)
                                .And(KcbDangkyKcb.Columns.LaPhidichvukemtheo).IsNotEqualTo(1).ExecuteAsCollection<KcbDangkyKcbCollection>();
                            foreach (KcbDangkyKcb _item in lstKcbDangkyKcb)
                            {
                                dongia = _item.DonGia;
                                if (_item.TuTuc == 0)
                                {
                                    bhytchitra = THU_VIEN_CHUNG.TinhBhytChitra(ptramBhyt, dongia, 0);
                                    bnhanchitra = dongia - bhytchitra;
                                }
                                else
                                {
                                    bhytchitra = 0;
                                    bnhanchitra = dongia;
                                }
                            }
                        }
                        else if (option == 2 || option == -1)
                        {
                        }
                        else if (option == 3 || option == -1)
                        {
                        }
                    }
                    scope.Complete();
                    return ActionResult.Success;
                }
            }
            catch
            {
                return ActionResult.Exception;
            }
        }
        public void HUYTHONGTIN_THANHTOAN(KcbThanhtoanChitietCollection objArrPaymentDetail, KcbThanhtoan objThanhtoan)
        {
            try
            {
                using (var scope = new TransactionScope())
                {
                    new Update(KcbDangkyKcb.Schema)
                        .Set(KcbDangkyKcb.Columns.IdThanhtoan).EqualTo(-1)
                        .Set(KcbDangkyKcb.Columns.NgayThanhtoan).EqualTo(null)
                        .Set(KcbDangkyKcb.Columns.TrangthaiThanhtoan).EqualTo(0)
                        .Set(KcbDangkyKcb.Columns.TileChietkhau).EqualTo(0)
                        .Set(KcbDangkyKcb.Columns.TienChietkhau).EqualTo(0)
                        .Set(KcbDangkyKcb.Columns.NgaySua).EqualTo(globalVariables.SysDate)
                        .Set(KcbDangkyKcb.Columns.NguoiSua).EqualTo(globalVariables.UserName)
                        .Where(KcbDangkyKcb.Columns.IdThanhtoan).IsEqualTo(objThanhtoan.IdThanhtoan).Execute();
                    new Update(NoitruPhanbuonggiuong.Schema)
                        .Set(NoitruPhanbuonggiuong.Columns.IdThanhtoan).EqualTo(-1)
                        .Set(NoitruPhanbuonggiuong.Columns.NgayThanhtoan).EqualTo(null)
                        .Set(NoitruPhanbuonggiuong.Columns.TrangthaiThanhtoan).EqualTo(0)
                        //.Set(NoitruPhanbuonggiuong.Columns.TileChietkhau).EqualTo(0)
                        //.Set(NoitruPhanbuonggiuong.Columns.TienChietkhau).EqualTo(0)
                        .Set(NoitruPhanbuonggiuong.Columns.NgaySua).EqualTo(globalVariables.SysDate)
                        .Set(NoitruPhanbuonggiuong.Columns.NguoiSua).EqualTo(globalVariables.UserName)
                        .Where(NoitruPhanbuonggiuong.Columns.IdThanhtoan).IsEqualTo(objThanhtoan.IdThanhtoan).Execute();


                    new Update(KcbChidinhclsChitiet.Schema)
                        .Set(KcbChidinhclsChitiet.Columns.TrangthaiChuyencls).EqualTo(0)
                        .Set(KcbChidinhclsChitiet.Columns.NgaySua).EqualTo(globalVariables.SysDate)
                        .Set(KcbChidinhclsChitiet.Columns.NguoiSua).EqualTo(globalVariables.UserName)
                        .Set(KcbChidinhclsChitiet.Columns.TrangthaiThanhtoan).EqualTo(0)
                         .Set(KcbChidinhclsChitiet.Columns.TileChietkhau).EqualTo(0)
                        .Set(KcbChidinhclsChitiet.Columns.TienChietkhau).EqualTo(0)
                        .Set(KcbChidinhclsChitiet.Columns.NgayThanhtoan).EqualTo(null)
                        .Set(KcbChidinhclsChitiet.Columns.IdThanhtoan).EqualTo(-1)
                        .Where(KcbChidinhclsChitiet.Columns.IdThanhtoan).IsEqualTo(objThanhtoan.IdThanhtoan).Execute();
                   
                   

                    new Update(KcbDonthuocChitiet.Schema)
                        .Set(KcbDonthuocChitiet.Columns.TrangthaiThanhtoan).EqualTo(0)
                        .Set(KcbDonthuocChitiet.Columns.NgayThanhtoan).EqualTo(null)
                        .Set(KcbDonthuocChitiet.Columns.IdThanhtoan).EqualTo(-1)
                         .Set(KcbDonthuocChitiet.Columns.TileChietkhau).EqualTo(0)
                        .Set(KcbDonthuocChitiet.Columns.TienChietkhau).EqualTo(0)
                         .Set(KcbDonthuocChitiet.Columns.NgaySua).EqualTo(globalVariables.SysDate)
                        .Set(KcbDonthuocChitiet.Columns.NguoiSua).EqualTo(globalVariables.UserName)
                        .Where(KcbDonthuocChitiet.Columns.IdThanhtoan).IsEqualTo(objThanhtoan.IdThanhtoan).Execute();

                    



                    new Update(TTongChiphi.Schema)
                    .Set(TTongChiphi.Columns.PaymentId).EqualTo(null)
                    .Set(TTongChiphi.Columns.PaymentStatus).EqualTo(0)
                    .Set(TTongChiphi.Columns.PaymentDate).EqualTo(null)
                    .Where(TTongChiphi.Columns.PaymentId).IsEqualTo(objThanhtoan.IdThanhtoan).Execute();


                    new Delete().From(KcbPhieuthu.Schema)
                        .Where(KcbPhieuthu.Columns.IdThanhtoan).IsEqualTo(objThanhtoan.IdThanhtoan).Execute();
                    new Delete().From(KcbThanhtoanChitiet.Schema)
                        .Where(KcbThanhtoanChitiet.Columns.IdThanhtoan).IsEqualTo(objThanhtoan.IdThanhtoan).Execute();
                    scope.Complete();
                }
            }
            catch (Exception exception)
            {
                log.Error(exception.ToString);
                // return ActionResult.Error;
            }
        }
        public ActionResult HuyThongTinLanThanhToan_Donthuoctaiquay(int id_thanhtoan, KcbLuotkham objLuotkham, string lydohuy, int IdHdonLog, bool HuyBienlai)
        {
            try
            {
                decimal v_TotalPaymentDetail = 0;
                decimal v_DiscountRate = 0;
                using (var scope = new TransactionScope())
                {
                    using (var dbscope = new SharedDbConnectionScope())
                    {
                        if (IdHdonLog > 0)
                            if (HuyBienlai)
                                new Update(HoadonLog.Schema).Set(HoadonLog.Columns.TrangThai).EqualTo(1)
                                    .Where(HoadonLog.Columns.IdHdonLog).IsEqualTo(IdHdonLog).Execute();
                            else
                                new Delete().From(HoadonLog.Schema)
                                    .Where(HoadonLog.Columns.IdHdonLog).IsEqualTo(IdHdonLog).Execute();
                        SqlQuery sqlQuery =
                            new Select().From(KcbThanhtoanChitiet.Schema).Where(KcbThanhtoanChitiet.Columns.IdThanhtoan).IsEqualTo(
                                id_thanhtoan);
                        KcbThanhtoanChitietCollection arrPaymentDetails = sqlQuery.ExecuteAsCollection<KcbThanhtoanChitietCollection>();
                        KcbThanhtoan objThanhtoan = KcbThanhtoan.FetchByID(id_thanhtoan);
                        int id_donthuoc = -1;
                        if (arrPaymentDetails.Count > 0) id_donthuoc = arrPaymentDetails[0].IdPhieu;
                        if (objThanhtoan != null)
                            HUYTHONGTIN_THANHTOAN(arrPaymentDetails, objThanhtoan);
                        KcbDonthuoc objDonthuoc = KcbDonthuoc.FetchByID(id_donthuoc);
                        KcbDonthuocChitietCollection lstChitiet = new Select().From(KcbDonthuoc.Schema).Where(KcbDonthuoc.Columns.IdDonthuoc).IsEqualTo(id_donthuoc).ExecuteAsCollection<KcbDonthuocChitietCollection>();
                        ActionResult actionResult = ActionResult.Success;
                        if (objDonthuoc != null && lstChitiet.Count > 0)
                        {
                           actionResult= new XuatThuoc().HuyXacNhanDonThuocBN(id_donthuoc, Utility.Int16Dbnull(lstChitiet[0].IdKho, 0));
                            switch (actionResult)
                            {
                                case ActionResult.Success:
                                    break;
                                case ActionResult.Error:
                                    return actionResult;
                            }
                        }
                        KcbThanhtoan.Delete(id_thanhtoan);
                    }
                    scope.Complete();
                    return ActionResult.Success;
                }
            }
            catch (Exception exception)
            {
                log.Error("Loi trong qua trinh huy thong tin {0}", exception.ToString());
                return ActionResult.Error;
            }

        }
        public ActionResult HuyThongTinLanThanhToan_Ao(int id_thanhtoan, KcbLuotkham objLuotkham, string lydohuy, int IdHdonLog, bool HuyBienlai)
        {
            try
            {
                decimal v_TotalPaymentDetail = 0;
                decimal v_DiscountRate = 0;
                using (var scope = new TransactionScope())
                {
                    using (var dbscope = new SharedDbConnectionScope())
                    {
                        if (IdHdonLog > 0)
                            if (HuyBienlai)
                                new Update(HoadonLog.Schema).Set(HoadonLog.Columns.TrangThai).EqualTo(1)
                                    .Where(HoadonLog.Columns.IdHdonLog).IsEqualTo(IdHdonLog).Execute();
                            else
                                new Delete().From(HoadonLog.Schema)
                                    .Where(HoadonLog.Columns.IdHdonLog).IsEqualTo(IdHdonLog).Execute();
                        SqlQuery sqlQuery =
                            new Select().From(KcbThanhtoanChitiet.Schema).Where(KcbThanhtoanChitiet.Columns.IdThanhtoan).IsEqualTo(
                                id_thanhtoan);
                        KcbThanhtoanChitietCollection arrPaymentDetails = sqlQuery.ExecuteAsCollection<KcbThanhtoanChitietCollection>();
                        KcbThanhtoan objThanhtoan = KcbThanhtoan.FetchByID(id_thanhtoan);
                        if (objThanhtoan != null)
                            HUYTHONGTIN_THANHTOAN(arrPaymentDetails, objThanhtoan);
                        new Delete().From(KcbPhieuDct.Schema)
                            .Where(KcbPhieuDct.Columns.MaLuotkham).IsEqualTo(objThanhtoan.MaLuotkham)
                            .And(KcbPhieuDct.Columns.IdBenhnhan).IsEqualTo(objThanhtoan.IdBenhnhan)
                            .And(KcbPhieuDct.Columns.LoaiThanhtoan).IsEqualTo(objThanhtoan.KieuThanhtoan).Execute();
                        if (objLuotkham != null)
                        {
                            byte locked = (byte)(objLuotkham.MaDoituongKcb == "DV" ? objLuotkham.Locked : 0);
                            new Update(KcbLuotkham.Schema)
                                .Set(KcbLuotkham.Columns.NgayKetthuc).EqualTo(null)
                                .Set(KcbLuotkham.Columns.NguoiKetthuc).EqualTo(string.Empty)
                                .Set(KcbLuotkham.Columns.Locked).EqualTo(locked)
                                .Set(KcbLuotkham.Columns.TrangthaiNgoaitru).EqualTo(locked)
                                .Set(KcbLuotkham.Columns.BoVien).EqualTo(0)
                                .Set(KcbLuotkham.Columns.LydoKetthuc).EqualTo("")
                                .Where(KcbLuotkham.Columns.MaLuotkham).IsEqualTo(objLuotkham.MaLuotkham)
                                .And(KcbLuotkham.Columns.IdBenhnhan).IsEqualTo(objLuotkham.IdBenhnhan).Execute();
                        }
                        KcbThanhtoan.Delete(id_thanhtoan);
                        if (objLuotkham != null) log.Info(string.Format("Phiếu thanh toán ID: {0} của bệnh nhân: {1} - ID Bệnh nhân: {2} đã được hủy bởi :{3} với lý do hủy :{4}", id_thanhtoan.ToString(), objLuotkham.MaLuotkham, objLuotkham.IdBenhnhan, globalVariables.UserName, lydohuy));
                    }
                    scope.Complete();
                    return ActionResult.Success;
                }
            }
            catch (Exception exception)
            {
                log.Error("Loi trong qua trinh huy thong tin {0}", exception.ToString());
                return ActionResult.Error;
            }

        }
        public ActionResult HuyThongTinLanThanhToan(int id_thanhtoan, KcbLuotkham objLuotkham, string lydohuy, int IdHdonLog,bool HuyBienlai)
        {
            try
            {
                decimal v_TotalPaymentDetail = 0;
                decimal v_DiscountRate = 0;
                using (var scope = new TransactionScope())
                {
                    using (var dbscope = new SharedDbConnectionScope())
                    {
                        if (IdHdonLog > 0)
                            if (HuyBienlai) 
                                new Update(HoadonLog.Schema).Set(HoadonLog.Columns.TrangThai).EqualTo(1)
                                    .Where(HoadonLog.Columns.IdHdonLog).IsEqualTo(IdHdonLog).Execute();
                            else
                                new Delete().From(HoadonLog.Schema)
                                    .Where(HoadonLog.Columns.IdHdonLog).IsEqualTo(IdHdonLog).Execute();
                        SqlQuery sqlQuery =
                            new Select().From(KcbThanhtoanChitiet.Schema).Where(KcbThanhtoanChitiet.Columns.IdThanhtoan).IsEqualTo(
                                id_thanhtoan);
                        KcbThanhtoanChitietCollection arrPaymentDetails = sqlQuery.ExecuteAsCollection<KcbThanhtoanChitietCollection>();
                        KcbThanhtoan objThanhtoan = KcbThanhtoan.FetchByID(id_thanhtoan);
                        if (objThanhtoan != null)
                            HUYTHONGTIN_THANHTOAN(arrPaymentDetails, objThanhtoan);
                        new Delete().From(KcbPhieuDct.Schema)
                            .Where(KcbPhieuDct.Columns.MaLuotkham).IsEqualTo(objThanhtoan.MaLuotkham)
                            .And(KcbPhieuDct.Columns.IdBenhnhan).IsEqualTo(objThanhtoan.IdBenhnhan)
                            .And(KcbPhieuDct.Columns.LoaiThanhtoan).IsEqualTo(objThanhtoan.KieuThanhtoan).Execute();
                        if (objLuotkham != null)
                        {
                            byte locked = (byte)(objLuotkham.MaDoituongKcb == "DV" ? objLuotkham.Locked : 0);
                            new Update(KcbLuotkham.Schema)
                                .Set(KcbLuotkham.Columns.NgayKetthuc).EqualTo(null)
                                .Set(KcbLuotkham.Columns.NguoiKetthuc).EqualTo(string.Empty)
                                .Set(KcbLuotkham.Columns.Locked).EqualTo(locked)
                                .Set(KcbLuotkham.Columns.TrangthaiNgoaitru).EqualTo(locked)
                                .Set(KcbLuotkham.Columns.LydoKetthuc).EqualTo("")
                                .Where(KcbLuotkham.Columns.MaLuotkham).IsEqualTo(objLuotkham.MaLuotkham)
                                .And(KcbLuotkham.Columns.IdBenhnhan).IsEqualTo(objLuotkham.IdBenhnhan).Execute();
                        }
                        KcbThanhtoan.Delete(id_thanhtoan);
                        if (objLuotkham != null) log.Info(string.Format("Phiếu thanh toán ID: {0} của bệnh nhân: {1} - ID Bệnh nhân: {2} đã được hủy bởi :{3} với lý do hủy :{4}", id_thanhtoan.ToString(), objLuotkham.MaLuotkham, objLuotkham.IdBenhnhan, globalVariables.UserName, lydohuy));
                    }
                    scope.Complete();
                    return ActionResult.Success;
                }
            }
            catch (Exception exception)
            {
                log.Error("Loi trong qua trinh huy thong tin {0}", exception.ToString());
                return ActionResult.Error;
            }

        }
        public DataTable Laychitietthanhtoan(int IdThanhtoan)
        {
            return SPs.KcbThanhtoanLaythongtinchitietTheoid(IdThanhtoan).GetDataSet().Tables[0];
        }
        public DataTable KiemtraTrangthaidonthuocTruockhihuythanhtoan(int IdThanhtoan)
        {
            return SPs.DonthuocKiemtraxacnhanthuocTrongdon(IdThanhtoan).GetDataSet().Tables[0];
        }
        public ActionResult UpdateHuyInPhoiBHYT(KcbLuotkham objLuotkham, KieuThanhToan kieuThanhToan)
        {
            try
            {
                using (var scope = new TransactionScope())
                {
                    using (var sh = new SharedDbConnectionScope())
                    {
                        new Update(KcbLuotkham.Schema)
                            //.Set(KcbLuotkham.Columns.TinhTrangRaVienStatus).EqualTo(0)
                            .Set(KcbLuotkham.Columns.NgayKetthuc).EqualTo(null)
                            .Set(KcbLuotkham.Columns.NguoiSua).EqualTo(globalVariables.UserName)
                            .Set(KcbLuotkham.Columns.NgaySua).EqualTo(globalVariables.SysDate)
                            //.Set(KcbLuotkham.Columns.IpMacSua).EqualTo(globalVariables.IpMacAddress)
                            //.Set(KcbLuotkham.Columns.IpMaySua).EqualTo(globalVariables.IpAddress)
                            //.Set(KcbLuotkham.Columns.ReasonBy).EqualTo("Hủy phôi bảo hiểm")
                            .Where(KcbLuotkham.Columns.MaLuotkham).IsEqualTo(objLuotkham.MaLuotkham)
                            .And(KcbLuotkham.Columns.IdBenhnhan).IsEqualTo(objLuotkham.IdBenhnhan)
                            .Execute();
                        new Delete().From(KcbPhieuDct.Schema)
                            .Where(KcbPhieuDct.Columns.MaLuotkham).IsEqualTo(objLuotkham.MaLuotkham)
                            .And(KcbPhieuDct.Columns.IdBenhnhan).IsEqualTo(objLuotkham.IdBenhnhan)
                            .And(KcbPhieuDct.Columns.LoaiThanhtoan).IsEqualTo(kieuThanhToan).Execute();

                    }
                    scope.Complete();
                    return ActionResult.Success;
                }
            }
            catch (Exception exception)
            {
                log.Error("Loi trong qua trinh tra tien lai:{0}", exception.ToString());
                return ActionResult.Error;
            }
        }
        private decimal SumOfPaymentDetail(KcbThanhtoanChitiet[] objArrPaymentDetail)
        {
            decimal SumOfPaymentDetail = 0;
            foreach (KcbThanhtoanChitiet paymentDetail in objArrPaymentDetail)
            {
                if (paymentDetail.TuTuc == 0)
                    SumOfPaymentDetail += (Utility.Int32Dbnull(paymentDetail.SoLuong) *
                                          Utility.DecimaltoDbnull(paymentDetail.DonGia))
                                          +
                                          (Utility.DecimaltoDbnull(paymentDetail.PhuThu, 0) *
                                          Utility.Int32Dbnull(paymentDetail.SoLuong, 0));
            }
            return SumOfPaymentDetail;
        }
        /// <summary>
        /// Trả lại tiền
        /// </summary>
        /// <param name="objThanhtoan"></param>
        /// <param name="objLuotkham"></param>
        /// <param name="objArrPaymentDetail"></param>
        /// <returns></returns>
        public ActionResult HUYTHANHTOAN_NGOAITRU(KcbThanhtoan objThanhtoan, KcbLuotkham objLuotkham, KcbThanhtoanChitiet[] objArrPaymentDetail)
        {
            decimal v_DiscountRate = 0;
            ///tổng tiền hiện tại truyền vào của lần payment đang thực hiện
            decimal v_TotalOrginPrice = 0;
            ///tổng tiền đã thanh toán
            decimal v_TotalPaymentDetail = 0;
            try
            {
                using (var scope = new TransactionScope())
                {
                    using (var sh = new SharedDbConnectionScope())
                    {

                        v_TotalOrginPrice = SumOfPaymentDetail(objArrPaymentDetail);
                        KcbThanhtoanCollection paymentCollection =
                            new KcbThanhtoanController().FetchByQuery(
                                KcbThanhtoan.CreateQuery().AddWhere(KcbThanhtoan.Columns.MaLuotkham, Comparison.Equals,
                                                                objLuotkham.MaLuotkham).AND(
                                                                    KcbThanhtoan.Columns.IdBenhnhan, Comparison.Equals,
                                                                    objLuotkham.IdBenhnhan));

                        foreach (KcbThanhtoan payment in paymentCollection)
                        {
                            KcbThanhtoanChitietCollection paymentDetailCollection =
                                new KcbThanhtoanChitietController().FetchByQuery(
                                    KcbThanhtoanChitiet.CreateQuery().AddWhere(KcbThanhtoanChitiet.Columns.IdThanhtoan, Comparison.Equals, payment.IdThanhtoan));
                            foreach (KcbThanhtoanChitiet paymentDetail in paymentDetailCollection)
                            {
                                if (paymentDetail.TuTuc == 0)
                                    v_TotalPaymentDetail += Utility.DecimaltoDbnull(paymentDetail.DonGia);

                            }

                        }
                        ///lấy thông tin chiết khấu xem đã thực hiện chưa
                        LayThongtinPtramBHYT(v_TotalPaymentDetail - v_TotalOrginPrice, objLuotkham, ref v_DiscountRate);
                        objThanhtoan.TrangThai = 1;
                        objThanhtoan.IdNhanvienThanhtoan = globalVariables.gv_intIDNhanvien;
                        objThanhtoan.NgayThanhtoan = globalVariables.SysDate;
                        //objThanhtoan.IpMayTao = THU_VIEN_CHUNG.GetIP4Address();
                        //objThanhtoan.IpMacTao = THU_VIEN_CHUNG.GetMACAddress();
                        // objThanhtoan.MaThanhtoan = THU_VIEN_CHUNG.GenerateMaThanhtoan(globalVariables.SysDate, 0);
                        objThanhtoan.MaThanhtoan = THU_VIEN_CHUNG.TaoMathanhtoan(globalVariables.SysDate);
                        objThanhtoan.IsNew = true;
                        objThanhtoan.Save();
                        //StoredProcedure spPament = SPs.KcbThanhtoanThemmoi(objThanhtoan.IdThanhtoan, objThanhtoan.MaLuotkham, objThanhtoan.IdBenhnhan,
                        //                 objThanhtoan.NgayThanhtoan, objThanhtoan.StaffId, objThanhtoan.Status,
                        //                 objThanhtoan.CreatedBy, objThanhtoan.CreatedDate, objThanhtoan.NgaySua,
                        //                 objThanhtoan.NguoiSua, objThanhtoan.MaThanhtoan, objThanhtoan.KieuThanhToan,
                        //                 objThanhtoan.DaIn, objThanhtoan.NgayIn, objThanhtoan.NgayTHop, objThanhtoan.NguoiIn,
                        //                 objThanhtoan.NguoiTHop, Utility.Int32Dbnull(objThanhtoan.TrongGoi), objThanhtoan.IpMayTao, objThanhtoan.IpMacTao, globalVariables.MA_KHOA_THIEN);
                        //spPament.Execute();
                        //objThanhtoan.IdThanhtoan = Utility.Int32Dbnull(spPament.OutputValues[0], -1);
                        //  objThanhtoan.IdThanhtoan = Utility.Int32Dbnull(_queryPayment.GetMax(KcbThanhtoan.Columns.IdThanhtoan), -1);

                        foreach (KcbThanhtoanChitiet objPayDetail in objArrPaymentDetail)
                        {
                            new Update(KcbThanhtoanChitiet.Schema)
                                //.Set(KcbThanhtoanChitiet.Columns.ng).EqualTo(1)

                                .Set(KcbThanhtoanChitiet.Columns.TrangthaiHuy).EqualTo(1)
                                .Set(KcbThanhtoanChitiet.Columns.NgayHuy).EqualTo(globalVariables.SysDate)
                                .Set(KcbThanhtoanChitiet.Columns.NguoiHuy).EqualTo(globalVariables.UserName)
                                .Where(KcbThanhtoanChitiet.Columns.IdChitiet).IsEqualTo(objPayDetail.IdChitiet).
                                Execute();
                            ///thanh toán khám chữa bệnh))
                            if (objPayDetail.IdLoaithanhtoan == 1)
                            {

                                new Update(KcbDangkyKcb.Schema)
                                    .Set(KcbDangkyKcb.Columns.TrangthaiThanhtoan).EqualTo(1)
                                    .Set(KcbDangkyKcb.Columns.TrangthaiHuy).EqualTo(1)
                                    .Where(KcbDangkyKcb.Columns.IdKham).IsEqualTo(objPayDetail.IdPhieu).Execute();
                            }
                            ///thah toán phần dịch vụ cận lâm sàng
                            if (objPayDetail.IdLoaithanhtoan == 2)
                            {
                                int status =
                                    Utility.Int32Dbnull(
                                        new Select(KcbChidinhclsChitiet.Columns.TrangThai).From(KcbChidinhclsChitiet.Schema).
                                            Where(KcbChidinhclsChitiet.Columns.IdChitietchidinh).IsEqualTo(objPayDetail.IdChitietdichvu)
                                            .ExecuteScalar().ToString(), 0);
                                if (globalVariables.UserName != "ADMIN")
                                {
                                    if (status == 1)
                                    {
                                        return ActionResult.AssginIsConfirmed;
                                    }
                                }
                                new Update(KcbChidinhclsChitiet.Schema)
                                    .Set(KcbChidinhclsChitiet.Columns.TrangthaiThanhtoan).EqualTo(1)
                                    .Set(KcbChidinhclsChitiet.Columns.TrangthaiHuy).EqualTo(1)
                                    .Where(KcbChidinhclsChitiet.Columns.IdChitietchidinh).IsEqualTo(objPayDetail.IdChitietdichvu)
                                    .Execute();
                            }
                            ///thanh toán phần thuốc
                            if (objPayDetail.IdLoaithanhtoan == 3)
                            {
                                int Status =
                                    Utility.Int32Dbnull(
                                        new Select(KcbDonthuoc.Columns.TrangThai).From(KcbDonthuoc.Schema).Where(
                                            KcbDonthuoc.Columns.IdDonthuoc).IsEqualTo(objPayDetail.IdPhieu).ExecuteScalar(),
                                        -1);

                                if (globalVariables.UserName != "ADMIN")
                                {
                                    if (Status == 3)
                                    {
                                        return ActionResult.PresIsConfirmed;
                                    }
                                }
                                new Update(KcbDonthuoc.Schema)
                                    // .Set(KcbDonthuoc.Columns.TrangthaiThanhtoan).EqualTo(1)
                                    .Set(KcbDonthuoc.Columns.TrangThai).EqualTo(0)
                                    .Where(KcbDonthuoc.Columns.IdDonthuoc).IsEqualTo(objPayDetail.IdPhieu).Execute();
                                new Update(KcbDonthuocChitiet.Schema)
                                    .Set(KcbDonthuocChitiet.Columns.TrangthaiHuy).EqualTo(1)
                                    .Set(KcbDonthuocChitiet.Columns.TrangthaiThanhtoan).EqualTo(1)
                                    .Where(KcbDonthuocChitiet.Columns.IdDonthuoc).IsEqualTo(objPayDetail.IdPhieu)
                                    .And(KcbDonthuocChitiet.Columns.IdThuoc).IsEqualTo(objPayDetail.IdDichvu)
                                    .Execute();
                            }
                            new Update(KcbThanhtoanChitiet.Schema)
                               .Set(KcbThanhtoanChitiet.Columns.NgayHuy).EqualTo(globalVariables.SysDate)
                               .Set(KcbThanhtoanChitiet.Columns.NguoiHuy).EqualTo(globalVariables.UserName)
                               .Set(KcbThanhtoanChitiet.Columns.TrangthaiHuy).EqualTo(1)
                               .Where(KcbThanhtoanChitiet.Columns.IdChitiet).IsEqualTo(objPayDetail.IdChitiet).
                               Execute();
                            objPayDetail.IdPhieu = Utility.Int32Dbnull(objPayDetail.IdThanhtoan);
                            objPayDetail.IdChitietdichvu = Utility.Int32Dbnull(objPayDetail.IdChitietdichvu);
                            //objPayDetail.IpMayTao = THU_VIEN_CHUNG.GetIP4Address();
                            //objPayDetail.IpMacTao = THU_VIEN_CHUNG.GetMACAddress();
                            objPayDetail.IdThanhtoan = Utility.Int32Dbnull(objThanhtoan.IdThanhtoan, -1);
                            objPayDetail.IsNew = true;
                            objPayDetail.Save();



                        }
                        KcbPhieuthu objPhieuthu = new KcbPhieuthu();
                        objPhieuthu.IdThanhtoan = Utility.Int32Dbnull(objThanhtoan.IdThanhtoan);
                        objPhieuthu.NgayThuchien = globalVariables.SysDate;
                        //objPhieuthu.IpMayTao = THU_VIEN_CHUNG.GetIP4Address();
                        objPhieuthu.MaPhieuthu = THU_VIEN_CHUNG.GetMaPhieuThu(globalVariables.SysDate, Utility.Int32Dbnull(objPhieuthu.LoaiPhieuthu, 0));
                        objPhieuthu.LoaiPhieuthu = 1;
                        KcbDanhsachBenhnhan objPatientInfo = KcbDanhsachBenhnhan.FetchByID(objThanhtoan.IdBenhnhan);
                        if (objPatientInfo != null)
                        {
                            objPhieuthu.NguoiNop = Utility.sDbnull(objPatientInfo.TenBenhnhan);

                        }
                        var query = (from loz in objArrPaymentDetail.AsEnumerable()
                                     select loz.TenChitietdichvu).ToArray();



                        decimal SoTien = Utility.DecimaltoDbnull(objArrPaymentDetail.Sum(c => c.BnhanChitra * c.SoLuong)) +
                                        Utility.DecimaltoDbnull(objArrPaymentDetail.Sum(c => c.PhuThu * c.SoLuong));

                        objPhieuthu.LydoNop = string.Join(";", query);
                        objPhieuthu.LoaiPhieuthu = 1;
                        objPhieuthu.SoTien = SoTien;
                        objPhieuthu.MaPhieuthu = THU_VIEN_CHUNG.GetMaPhieuThu(globalVariables.SysDate, Utility.Int32Dbnull(objPhieuthu.LoaiPhieuthu));
                        objPhieuthu.IsNew = true;
                        objPhieuthu.Save();
                        //CAN XEM LAIJ PHAN THU TUC
                        //StoredProcedure sp = SPs.KcbThanhtoanThemmoiPhieuthu(objPhieuthu.MaPthu, objThanhtoan.IdThanhtoan,
                        //                                             objPhieuthu.NgayThien,
                        //                                             objPhieuthu.NguoiNop, objPhieuthu.LdoNop,
                        //                                             objPhieuthu.SoTien,
                        //                                             objPhieuthu.SluongCtuGoc, objPhieuthu.TkhoanNo,
                        //                                             objPhieuthu.TkhoanCo,
                        //                                             objPhieuthu.LoaiPhieu, globalVariables.UserName,
                        //                                             globalVariables.SysDate,
                        //                                             globalVariables.gv_intIDNhanvien,
                        //                                             globalVariables.DepartmentID,
                        //                                             globalVariables.UserName, globalVariables.SysDate);

                        //sp.Execute();

                    }
                    scope.Complete();
                    return ActionResult.Success;
                }
            }
            catch (Exception exception)
            {
                log.Error("Loi trong qua trinh tra tien lai:{0}", exception.ToString());
                return ActionResult.Error;
            }

        }
        public ActionResult UpdatePhieuDCT(KcbPhieuDct objPhieuDct, KcbLuotkham objLuotkham)
        {
            try
            {
                using (var scope = new TransactionScope())
                {
                    using (var sh = new SharedDbConnectionScope())
                    {
                        decimal PtramBHYT = 0;
                        SqlQuery sqlQuery = new Select().From(KcbThanhtoanChitiet.Schema)
                            .Where(KcbThanhtoanChitiet.Columns.IdThanhtoan).In(
                                new Select(KcbThanhtoan.Columns.IdThanhtoan).From(KcbThanhtoan.Schema).Where(
                                    KcbThanhtoan.Columns.MaLuotkham).IsEqualTo(
                                        objLuotkham.MaLuotkham).And(KcbThanhtoan.Columns.IdBenhnhan).IsEqualTo(
                                            objLuotkham.IdBenhnhan).And(KcbThanhtoan.Columns.KieuThanhtoan).IsEqualTo(
                                                KieuThanhToan.NgoaiTru).And(KcbThanhtoan.Columns.TrangThai).IsEqualTo(0))
                            .And(KcbThanhtoanChitiet.Columns.TrangthaiHuy).IsEqualTo(0)
                            .And(KcbThanhtoanChitiet.Columns.TuTuc).IsEqualTo(0);

                        KcbThanhtoanChitietCollection objThanhtoanDetailCollection =
                            sqlQuery.ExecuteAsCollection<KcbThanhtoanChitietCollection>();
                        decimal TongTien =
                            Utility.DecimaltoDbnull(objThanhtoanDetailCollection.Sum(c => c.SoLuong * c.DonGia));
                        LayThongtinPtramBHYT(TongTien, objLuotkham, ref PtramBHYT);
                        if (Utility.DecimaltoDbnull(objLuotkham.PtramBhyt) != PtramBHYT)
                        {
                            objLuotkham.PtramBhyt = PtramBHYT;
                            new Update(KcbLuotkham.Schema)
                           .Set(KcbLuotkham.Columns.NgaySua).EqualTo(globalVariables.SysDate)
                           .Set(KcbLuotkham.Columns.NguoiSua).EqualTo(globalVariables.UserName)
                           .Set(KcbLuotkham.Columns.PtramBhyt).EqualTo(PtramBHYT)
                           .Where(KcbLuotkham.Columns.MaLuotkham).IsEqualTo(objPhieuDct.MaLuotkham)
                           .And(KcbLuotkham.Columns.IdBenhnhan).IsEqualTo(objPhieuDct.IdBenhnhan).Execute();
                        }
                        if (globalVariables.gv_strTuyenBHYT == "TW")
                        {
                            foreach (KcbThanhtoanChitiet objThanhtoanDetail in objThanhtoanDetailCollection)
                            {
                                decimal BHCT = Utility.DecimaltoDbnull(objThanhtoanDetail.DonGia * PtramBHYT / 100);
                                decimal BNCT = Utility.DecimaltoDbnull(objThanhtoanDetail.DonGia - BHCT);
                                objThanhtoanDetail.BnhanChitra = BNCT;
                                objThanhtoanDetail.PtramBhyt = PtramBHYT;
                                objThanhtoanDetail.BhytChitra = BHCT;

                            }
                            objThanhtoanDetailCollection.SaveAll();
                        }
                        if (objThanhtoanDetailCollection.Count() > 0)
                        {
                            objPhieuDct.TongTien = objThanhtoanDetailCollection.Sum(c => c.DonGia * c.SoLuong);
                            objPhieuDct.BnhanChitra = objThanhtoanDetailCollection.Sum(c => c.BnhanChitra * c.SoLuong);
                            objPhieuDct.BhytChitra = objThanhtoanDetailCollection.Sum(c => c.BhytChitra * c.SoLuong);
                            sqlQuery = new Select().From<KcbPhieuDct>()
                                .Where(KcbPhieuDct.Columns.MaLuotkham)
                                .IsEqualTo(objPhieuDct.MaLuotkham)
                                .And(KcbPhieuDct.Columns.IdBenhnhan).IsEqualTo(objPhieuDct.IdBenhnhan)
                                .And(KcbPhieuDct.Columns.LoaiThanhtoan).IsEqualTo(objPhieuDct.LoaiThanhtoan);
                            if (sqlQuery.GetRecordCount() <= 0)
                            {
                                objPhieuDct.IsNew = true;
                                objPhieuDct.Save();
                                //StoredProcedure sp = SPs.BhytThemmoiPhieudct(objPhieuDct.IdPhieuDct, objPhieuDct.MaLuotkham,
                                //                                  objPhieuDct.IdBenhnhan, objPhieuDct.TienGoc,
                                //                                  objPhieuDct.TienBhct, objPhieuDct.TienBnct,
                                //                                  objPhieuDct.NguoiTao, objPhieuDct.NgayTao, objPhieuDct.KieuThanhtoan, globalVariables.MA_KHOA_THIEN);
                                //sp.Execute();
                                objLuotkham.TrangthaiNgoaitru = 1;
                                objLuotkham.Locked = 1;
                                objLuotkham.NgayKetthuc = objPhieuDct.NgayTao;
                                new Update(KcbLuotkham.Schema)
                                    //.Set(KcbLuotkham.Columns.IpMacSua).EqualTo(globalVariables.IpMacAddress)
                                    //.Set(KcbLuotkham.Columns.IpMaySua).EqualTo(globalVariables.IpAddress)
                                    .Set(KcbLuotkham.Columns.NgaySua).EqualTo(globalVariables.SysDate)
                                    .Set(KcbLuotkham.Columns.NguoiSua).EqualTo(globalVariables.UserName)
                                    .Set(KcbLuotkham.Columns.Locked).EqualTo(objLuotkham.Locked)
                                    .Set(KcbLuotkham.Columns.NgayKetthuc).EqualTo(objLuotkham.NgayKetthuc)
                                    .Set(KcbLuotkham.Columns.TrangthaiNgoaitru).EqualTo(Utility.Int32Dbnull(objLuotkham.TrangthaiNgoaitru))
                                    .Set(KcbLuotkham.Columns.LydoKetthuc).EqualTo("In phôi bảo hiểm")
                                     .Set(KcbLuotkham.Columns.IpMaysua).EqualTo(objPhieuDct.IpMaysua)
                                    .Set(KcbLuotkham.Columns.TenMaysua).EqualTo(objPhieuDct.TenMaysua)
                                    .Where(KcbLuotkham.Columns.MaLuotkham).IsEqualTo(objPhieuDct.MaLuotkham)
                                    .And(KcbLuotkham.Columns.IdBenhnhan).IsEqualTo(objPhieuDct.IdBenhnhan).Execute();
                            }
                            else
                            {
                                new Update(KcbPhieuDct.Schema)
                                    .Set(KcbPhieuDct.Columns.TongTien).EqualTo(objPhieuDct.TongTien)
                                    .Set(KcbPhieuDct.Columns.BnhanChitra).EqualTo(objPhieuDct.BnhanChitra)
                                    .Set(KcbPhieuDct.Columns.BhytChitra).EqualTo(objPhieuDct.BhytChitra)
                                    .Set(KcbPhieuDct.Columns.NguoiSua).EqualTo(globalVariables.UserName)
                                     .Set(KcbPhieuDct.Columns.IpMaysua).EqualTo(objPhieuDct.IpMaysua)
                                    .Set(KcbPhieuDct.Columns.TenMaysua).EqualTo(objPhieuDct.TenMaysua)
                                    .Where(KcbPhieuDct.Columns.MaLuotkham).IsEqualTo(objPhieuDct.MaLuotkham)
                                    .And(KcbPhieuDct.Columns.IdBenhnhan).IsEqualTo(objPhieuDct.IdBenhnhan)
                                    .And(KcbPhieuDct.Columns.LoaiThanhtoan).IsEqualTo(Utility.Int32Dbnull(objPhieuDct.LoaiThanhtoan))
                                    .Execute();
                            }
                        }
                    }
                    scope.Complete();
                    return ActionResult.Success;
                }
            }
            catch (Exception exception)
            {
                log.Error("Loi trong qua trinh tra tien lai:{0}", exception.ToString());
                return ActionResult.Error;
            }
        }
        public ActionResult HUYTHONGTIN_PHIEUCHI_NGOAITRU(KcbThanhtoan objThanhtoan)
        {
            try
            {
                using (var scope = new TransactionScope())
                {
                    using (var dbscope = new SharedDbConnectionScope())
                    {
                        if (objThanhtoan != null)
                        {
                            new Delete().From(KcbThanhtoan.Schema)
                                .Where(KcbThanhtoan.Columns.IdThanhtoan).IsEqualTo(objThanhtoan.IdThanhtoan).Execute();
                            new Delete().From(KcbThanhtoanChitiet.Schema)
                                .Where(KcbThanhtoanChitiet.Columns.IdThanhtoan).IsEqualTo(objThanhtoan.IdThanhtoan).Execute();
                            //new Delete().From(KydongKcbThanhtoanChitiet.Schema)
                            //     .Where(KydongKcbThanhtoanChitiet.Columns.IdThanhtoan).IsEqualTo(objThanhtoan.IdThanhtoan).Execute();
                            new Delete().From(KcbPhieuthu.Schema)
                                .Where(KcbPhieuthu.Columns.IdThanhtoan).IsEqualTo(objThanhtoan.IdThanhtoan)
                                .And(KcbPhieuthu.Columns.LoaiPhieuthu).IsEqualTo(1).Execute();
                            //new Delete().From(TTralaiTtoan.Schema)
                            //    .Where(TTralaiTtoan.Columns.IdThanhtoan).IsEqualTo(objThanhtoan.IdThanhtoan).Execute();
                        }
                        else
                        {
                            return ActionResult.Error;
                        }


                    }
                    scope.Complete();
                    return ActionResult.Success;

                }
            }
            catch (Exception exception)
            {
                log.Error("Ban ra loi Exception={0}", exception);
                return ActionResult.Error;
            }

        }
        public ActionResult UpdateNgayThanhtoan(KcbThanhtoan objThanhtoan)
        {
            try
            {
                using (var scope = new TransactionScope())
                {
                    using (var dbscope = new SharedDbConnectionScope())
                    {
                        new Update(KcbThanhtoan.Schema)
                            .Set(KcbThanhtoan.Columns.NgayThanhtoan).EqualTo(objThanhtoan.NgayThanhtoan)
                            .Where(KcbThanhtoan.Columns.IdThanhtoan).IsEqualTo(objThanhtoan.IdThanhtoan).Execute();
                        new Update(KcbDangkyKcb.Schema)
                            .Set(KcbDangkyKcb.Columns.NgayThanhtoan).EqualTo(objThanhtoan.NgayThanhtoan)
                            .Where(KcbDangkyKcb.Columns.IdThanhtoan).IsEqualTo(objThanhtoan.IdThanhtoan).Execute();
                        new Update(KcbChidinhclsChitiet.Schema)
                            .Set(KcbChidinhclsChitiet.Columns.NgayThanhtoan).EqualTo(objThanhtoan.NgayThanhtoan)
                            .Where(KcbChidinhclsChitiet.Columns.IdThanhtoan).IsEqualTo(objThanhtoan.IdThanhtoan).Execute();
                        new Update(KcbDonthuocChitiet.Schema)
                            .Set(KcbDonthuocChitiet.Columns.NgayThanhtoan).EqualTo(objThanhtoan.NgayThanhtoan)
                            .Where(KcbDonthuocChitiet.Columns.IdThanhtoan).IsEqualTo(objThanhtoan.IdThanhtoan).Execute();
                        //new Update(TPatientDept.Schema)
                        //    .Set(TPatientDept.Columns.NgayThanhtoan).EqualTo(objThanhtoan.NgayThanhtoan)
                        //    .Where(TPatientDept.Columns.IdThanhtoan).IsEqualTo(objThanhtoan.IdThanhtoan).Execute();
                        new Update(TTongChiphi.Schema)
                            .Set(TTongChiphi.Columns.PaymentDate).EqualTo(objThanhtoan.NgayThanhtoan)
                            .Where(TTongChiphi.Columns.PaymentId).IsEqualTo(objThanhtoan.IdThanhtoan).Execute();
                        //new Update(TDeposit.Schema)
                        //  .Set(TDeposit.Columns.NgayThanhtoan).EqualTo(objThanhtoan.NgayThanhtoan)
                        //  .Where(TDeposit.Columns.IdThanhtoan).IsEqualTo(objThanhtoan.IdThanhtoan).Execute();

                    }
                    scope.Complete();
                    return ActionResult.Success;
                }
            }
            catch (Exception exception)
            {
                // log.Error("Loi trong qua trinh huy thong tin {0}",exception.ToString());
                return ActionResult.Error;
            }
        }
        private void UpdatePaymentStatus(KcbThanhtoan objThanhtoan, KcbThanhtoanChitiet objThanhtoanDetail)
        {
            using (var scope = new TransactionScope())
            {
                switch (objThanhtoanDetail.IdLoaithanhtoan)
                {
                    case 1://Phí KCB
                        new Update(KcbDangkyKcb.Schema)
                            .Set(KcbDangkyKcb.Columns.IdThanhtoan).EqualTo(objThanhtoan.IdThanhtoan)
                            .Set(KcbDangkyKcb.Columns.TrangthaiThanhtoan).EqualTo(1)
                             .Set(KcbDangkyKcb.Columns.NgaySua).EqualTo(globalVariables.SysDate)
                            .Set(KcbDangkyKcb.Columns.NguoiSua).EqualTo(globalVariables.UserName)
                            .Set(KcbDangkyKcb.Columns.NgayThanhtoan).EqualTo(objThanhtoan.NgayThanhtoan)
                              .Set(KcbChidinhclsChitiet.Columns.TienChietkhau).EqualTo(objThanhtoanDetail.TienChietkhau)
                            .Set(KcbChidinhclsChitiet.Columns.TileChietkhau).EqualTo(objThanhtoanDetail.TileChietkhau)
                            .Set(KcbChidinhclsChitiet.Columns.KieuChietkhau).EqualTo(objThanhtoanDetail.KieuChietkhau)
                            .Where(KcbDangkyKcb.Columns.IdKham).IsEqualTo(objThanhtoanDetail.IdPhieu).Execute();

                        new Update(NoitruPhanbuonggiuong.Schema)
                            .Set(NoitruPhanbuonggiuong.Columns.IdThanhtoan).EqualTo(objThanhtoan.IdThanhtoan)
                            .Set(NoitruPhanbuonggiuong.Columns.TrangthaiThanhtoan).EqualTo(1)
                             .Set(NoitruPhanbuonggiuong.Columns.NgaySua).EqualTo(globalVariables.SysDate)
                            .Set(NoitruPhanbuonggiuong.Columns.NguoiSua).EqualTo(globalVariables.UserName)
                            .Set(NoitruPhanbuonggiuong.Columns.NgayThanhtoan).EqualTo(objThanhtoan.NgayThanhtoan)
                            //  .Set(KcbChidinhclsChitiet.Columns.TienChietkhau).EqualTo(objThanhtoanDetail.TienChietkhau)
                            //.Set(KcbChidinhclsChitiet.Columns.TileChietkhau).EqualTo(objThanhtoanDetail.TileChietkhau)
                            //.Set(KcbChidinhclsChitiet.Columns.KieuChietkhau).EqualTo(objThanhtoanDetail.KieuChietkhau)
                            .Where(NoitruPhanbuonggiuong.Columns.IdKham).IsEqualTo(objThanhtoanDetail.IdPhieu)
                            .And(NoitruPhanbuonggiuong.Columns.NoiTru).IsEqualTo(0).Execute();
                        break;
                    case 2://Phí CLS
                        new Update(KcbChidinhclsChitiet.Schema)
                            .Set(KcbChidinhclsChitiet.Columns.IdThanhtoan).EqualTo(objThanhtoan.IdThanhtoan)
                            .Set(KcbChidinhclsChitiet.Columns.TrangthaiThanhtoan).EqualTo(1)
                            .Set(KcbChidinhclsChitiet.Columns.TrangthaiChuyencls).EqualTo(1)
                             .Set(KcbChidinhclsChitiet.Columns.TienChietkhau).EqualTo(objThanhtoanDetail.TienChietkhau)
                            .Set(KcbChidinhclsChitiet.Columns.TileChietkhau).EqualTo(objThanhtoanDetail.TileChietkhau)
                            .Set(KcbChidinhclsChitiet.Columns.KieuChietkhau).EqualTo(objThanhtoanDetail.KieuChietkhau)
                            .Set(KcbChidinhclsChitiet.Columns.NgaySua).EqualTo(globalVariables.SysDate)
                            .Set(KcbChidinhclsChitiet.Columns.NguoiSua).EqualTo(globalVariables.UserName)
                            .Set(KcbChidinhclsChitiet.Columns.NgayThanhtoan).EqualTo(objThanhtoan.NgayThanhtoan)
                            .Where(KcbChidinhclsChitiet.Columns.IdChitietchidinh).IsEqualTo(objThanhtoanDetail.IdPhieuChitiet).Execute();
                        new Update(KcbChidinhcl.Schema)
                        .Set(KcbChidinhcl.Columns.TrangthaiThanhtoan).EqualTo(1)
                        .Set(KcbChidinhcl.Columns.NgayThanhtoan).EqualTo(objThanhtoan.NgayThanhtoan)
                        .Where(KcbChidinhcl.Columns.IdChidinh).IsEqualTo(objThanhtoanDetail.IdPhieu).Execute();
                        break;
                    case 3://Đơn thuốc ngoại trú
                    case 5://Đơn thuốc nội trú
                        new Update(KcbDonthuocChitiet.Schema)
                            .Set(KcbDonthuocChitiet.Columns.IdThanhtoan).EqualTo(objThanhtoan.IdThanhtoan)
                            .Set(KcbDonthuocChitiet.Columns.TrangthaiThanhtoan).EqualTo(1)
                            .Set(KcbDonthuocChitiet.Columns.NgayThanhtoan).EqualTo(objThanhtoan.NgayThanhtoan)
                             .Set(KcbDonthuocChitiet.Columns.TienChietkhau).EqualTo(objThanhtoanDetail.TienChietkhau)
                            .Set(KcbDonthuocChitiet.Columns.TileChietkhau).EqualTo(objThanhtoanDetail.TileChietkhau)
                            .Set(KcbDonthuocChitiet.Columns.KieuChietkhau).EqualTo(objThanhtoanDetail.KieuChietkhau)
                             .Set(KcbDonthuocChitiet.Columns.NgaySua).EqualTo(globalVariables.SysDate)
                            .Set(KcbDonthuocChitiet.Columns.NguoiSua).EqualTo(globalVariables.UserName)
                            .Where(KcbDonthuocChitiet.Columns.IdChitietdonthuoc).IsEqualTo(objThanhtoanDetail.IdPhieuChitiet).Execute();

                        new Update(KcbDonthuoc.Schema)
                           .Set(KcbDonthuoc.Columns.TrangthaiThanhtoan).EqualTo(1)
                           .Set(KcbDonthuoc.Columns.NgayThanhtoan).EqualTo(objThanhtoan.NgayThanhtoan)
                           .Where(KcbDonthuoc.Columns.IdDonthuoc).IsEqualTo(objThanhtoanDetail.IdPhieu).Execute();
                        break;

                    case 4:
                    //new Update(TPatientDept.Schema)
                    //    .Set(TPatientDept.Columns.IdThanhtoan).EqualTo(objThanhtoan.IdThanhtoan)
                    //    .Set(TPatientDept.Columns.TrangthaiThanhtoan).EqualTo(1)
                    //    .Set(TPatientDept.Columns.NgayThanhtoan).EqualTo(objThanhtoan.NgayThanhtoan)
                    //    .Where(TPatientDept.Columns.PatientDeptId).IsEqualTo(objThanhtoanDetail.Id).Execute();
                    //break;
                    case 0://Phí dịch vụ kèm theo
                        new Update(KcbDangkyKcb.Schema)
                          .Set(KcbDangkyKcb.Columns.IdThanhtoan).EqualTo(objThanhtoan.IdThanhtoan)
                          .Set(KcbDangkyKcb.Columns.TrangthaiThanhtoan).EqualTo(1)
                          .Set(KcbDangkyKcb.Columns.NgayThanhtoan).EqualTo(objThanhtoan.NgayThanhtoan)
                            .Set(KcbDangkyKcb.Columns.TienChietkhau).EqualTo(objThanhtoanDetail.TienChietkhau)
                            .Set(KcbDangkyKcb.Columns.TileChietkhau).EqualTo(objThanhtoanDetail.TileChietkhau)
                            .Set(KcbDangkyKcb.Columns.KieuChietkhau).EqualTo(objThanhtoanDetail.KieuChietkhau)
                              .Set(KcbDangkyKcb.Columns.NgaySua).EqualTo(globalVariables.SysDate)
                            .Set(KcbDangkyKcb.Columns.NguoiSua).EqualTo(globalVariables.UserName)
                          .Where(KcbDangkyKcb.Columns.IdKham).IsEqualTo(objThanhtoanDetail.IdPhieu)
                          .And(KcbDangkyKcb.Columns.LaPhidichvukemtheo).IsEqualTo(1)
                          .Execute();
                        break;
                }
                scope.Complete();
            }
        }
        public ActionResult UpdateICD10(KcbLuotkham objLuotkham,string ICDCode)
        {
            try
            {
                using (var scope = new TransactionScope())
                {
                    using (var dbscope = new SharedDbConnectionScope())
                    {

                       
                    }
                    scope.Complete();
                    return ActionResult.Success;

                }
            }
            catch (Exception exception)
            {
                log.InfoException("Ban ra loi exception=", exception);
                return ActionResult.Error;
            }

        }
        public ActionResult Capnhattrangthaithanhtoan(long IdThanhtoan)
        {
            try
            {
                using (var scope = new TransactionScope())
                {
                    using (var dbscope = new SharedDbConnectionScope())
                    {

                        new Update(KcbThanhtoan.Schema)
                           .Set(KcbThanhtoan.Columns.NguoiIn).EqualTo(globalVariables.UserName)
                           .Set(KcbThanhtoan.Columns.NgayIn).EqualTo(globalVariables.SysDate)
                           .Set(KcbThanhtoan.Columns.TrangthaiIn).EqualTo(1)
                           .Where(KcbThanhtoan.Columns.IdThanhtoan).IsEqualTo(IdThanhtoan).Execute();
                    }
                    scope.Complete();
                    return ActionResult.Success;

                }
            }
            catch (Exception exception)
            {
                log.InfoException("Ban ra loi exception=", exception);
                return ActionResult.Error;
            }

        }
        public ActionResult UpdateDataPhieuThu(KcbPhieuthu objPhieuthu)
        {
            try
            {
                using (var scope = new TransactionScope())
                {
                    using (var dbscope = new SharedDbConnectionScope())
                    {

                        StoredProcedure sp = SPs.KcbThanhtoanThemmoiPhieuthu(objPhieuthu.MaPhieuthu, objPhieuthu.IdThanhtoan,
                                                                    objPhieuthu.NgayThuchien,
                                                                    objPhieuthu.NguoiNop, objPhieuthu.LydoNop,
                                                                    objPhieuthu.SoTien, objPhieuthu.SotienGoc, objPhieuthu.TienChietkhau, objPhieuthu.TienChietkhauchitiet, objPhieuthu.TienChietkhauhoadon,
                                                                    objPhieuthu.SoluongChungtugoc, objPhieuthu.TaikhoanNo,
                                                                    objPhieuthu.TaikhoanCo,
                                                                    objPhieuthu.LoaiPhieuthu, globalVariables.UserName,
                                                                    globalVariables.SysDate,
                                                                    globalVariables.gv_intIDNhanvien,
                                                                    globalVariables.idKhoatheoMay,
                                                                    globalVariables.UserName, globalVariables.SysDate);
                        sp.Execute();

                        new Update(KcbThanhtoan.Schema)
                           .Set(KcbThanhtoan.Columns.NguoiIn).EqualTo(globalVariables.UserName)
                           .Set(KcbThanhtoan.Columns.NgayIn).EqualTo(globalVariables.SysDate)
                           .Set(KcbThanhtoan.Columns.TrangthaiIn).EqualTo(1)
                           .Where(KcbThanhtoan.Columns.IdThanhtoan).IsEqualTo(objPhieuthu.IdThanhtoan).Execute();

                    }
                    scope.Complete();
                    return ActionResult.Success;

                }
            }
            catch (Exception exception)
            {
                log.InfoException("Ban ra loi exception=", exception);
                return ActionResult.Error;
            }

        }
        public ActionResult UpdateDataPhieuThu(KcbPhieuthu objPhieuthu, KcbThanhtoanChitiet[] arrPaymentDetail)
        {
            try
            {
                using (var scope = new TransactionScope())
                {
                    using (var dbscope = new SharedDbConnectionScope())
                    {
                        StoredProcedure sp = SPs.KcbThanhtoanThemmoiPhieuthu( objPhieuthu.MaPhieuthu, objPhieuthu.IdThanhtoan,
                                                                     objPhieuthu.NgayThuchien,
                                                                     objPhieuthu.NguoiNop, objPhieuthu.LydoNop,
                                                                     objPhieuthu.SoTien, objPhieuthu.SotienGoc, objPhieuthu.TienChietkhau, objPhieuthu.TienChietkhauchitiet, objPhieuthu.TienChietkhauhoadon,
                                                                     objPhieuthu.SoluongChungtugoc, objPhieuthu.TaikhoanNo,
                                                                     objPhieuthu.TaikhoanCo,
                                                                     objPhieuthu.LoaiPhieuthu, globalVariables.UserName,
                                                                     globalVariables.SysDate,
                                                                     globalVariables.gv_intIDNhanvien,
                                                                     globalVariables.idKhoatheoMay,
                                                                     globalVariables.UserName, globalVariables.SysDate);
                        sp.Execute();
                        foreach (KcbThanhtoanChitiet objThanhtoanDetail in arrPaymentDetail)
                        {
                            new Update(KcbThanhtoanChitiet.Schema)
                                .Set(KcbThanhtoanChitiet.Columns.SttIn).EqualTo(objThanhtoanDetail.SttIn)
                                // .Set(KcbThanhtoanChitiet.Columns.PhuThu).EqualTo(objThanhtoanDetail.PhuThu)
                                .Where(KcbThanhtoanChitiet.Columns.IdChitiet).IsEqualTo(
                                    objThanhtoanDetail.IdChitiet).Execute();
                            log.Info("Cạp nhạp lại thong tin cua voi ma ID=" + objThanhtoanDetail.IdChitiet);
                        }
                        new Update(KcbThanhtoan.Schema)
                            .Set(KcbThanhtoan.Columns.NguoiIn).EqualTo(globalVariables.UserName)
                            .Set(KcbThanhtoan.Columns.NgayIn).EqualTo(globalVariables.SysDate)
                            .Set(KcbThanhtoan.Columns.TrangthaiIn).EqualTo(1)
                            .Where(KcbThanhtoan.Columns.IdThanhtoan).IsEqualTo(objPhieuthu.IdThanhtoan).Execute();

                    }
                    scope.Complete();
                    return ActionResult.Success;

                }
            }
            catch (Exception exception)
            {
                log.Error("Ban ra loi Exception={0}", exception);
                return ActionResult.Error;
            }

        }
        public DataTable GetDataInphieuDichvu(KcbThanhtoan objThanhtoan)
        {
            return
                SPs.KcbThanhtoanLaythongtinInphieuDichvu(Utility.Int32Dbnull(objThanhtoan.IdThanhtoan, -1), objThanhtoan.MaLuotkham,
                                      Utility.Int32Dbnull(objThanhtoan.IdBenhnhan)).GetDataSet().Tables[0];

        }
        public DataTable GetDataInphieuBH(KcbThanhtoan objThanhtoan, bool IsBH)
        {
            DataTable dataTable =
                SPs.BhytLaythongtinInphieubhyt(Utility.Int32Dbnull(objThanhtoan.IdThanhtoan, -1), objThanhtoan.MaLuotkham,
                                      Utility.Int32Dbnull(objThanhtoan.IdBenhnhan)).GetDataSet().Tables[0];
            if (IsBH)
            {
                foreach (DataRow drv in dataTable.Rows)
                {
                    if (drv["TuTuc"].ToString() == "1") drv.Delete();
                }
                dataTable.AcceptChanges();
            }
            return dataTable;
        }
        public DataTable INPHIEUBH_CHOBENHNHAN(KcbThanhtoan objThanhtoan)
        {
            //DataTable dataTable =
            //    SPs.BhytLaythongtinInphieubhytChobenhnhan(Utility.Int32Dbnull(objThanhtoan.IdThanhtoan, -1), objThanhtoan.MaLuotkham,
            //                          Utility.Int32Dbnull(objThanhtoan.IdBenhnhan)).GetDataSet().Tables[0];

            //return dataTable;
            return null;
        }
        public DataTable KYDONG_GetDataInphieuBH(KcbThanhtoan objThanhtoan, bool TuTuc)
        {
            return null;
            //DataTable dataTable =
            //    SPs.BhytLaythongtinInphieuTraituyen(Utility.Int32Dbnull(objThanhtoan.IdThanhtoan, -1), objThanhtoan.MaLuotkham,
            //                          Utility.Int32Dbnull(objThanhtoan.IdBenhnhan)).GetDataSet().Tables[0];
            //if (!TuTuc)
            //{
            //    foreach (DataRow drv in dataTable.Rows)
            //    {
            //        if (drv["TuTuc"].ToString() == "1") drv.Delete();
            //    }
            //    dataTable.AcceptChanges();
            //}
            //return dataTable;

        }
        public DataTable KYDONG_GetDataInphieuBH_TraiTuyen(KcbThanhtoan objThanhtoan)
        {
            return null;
            //return
            //    SPs.BhytLaythongtinInphieuTraituyen(Utility.Int32Dbnull(objThanhtoan.IdThanhtoan, -1), objThanhtoan.MaLuotkham,
            //                          Utility.Int32Dbnull(objThanhtoan.IdBenhnhan)).GetDataSet().Tables[0];

        }
        public void XuLyThongTinPhieu_DichVu(ref DataTable m_dtReportPhieuThu)
        {
            Utility.AddColumToDataTable(ref  m_dtReportPhieuThu, "TONG_BN", typeof(decimal));
            Utility.AddColumToDataTable(ref  m_dtReportPhieuThu, "PHU_THU", typeof(decimal));
            foreach (DataRow drv in m_dtReportPhieuThu.Rows)
            {
                //drv["ThanhTien"] = Utility.Int32Dbnull(drv["SoLuong"], 0) *
                //                   Utility.DecimaltoDbnull(drv["Discount_Price"], 0);
                drv["TotalSurcharge_Price"] = Utility.Int32Dbnull(drv["SoLuong"], 0) *
                                              Utility.DecimaltoDbnull(drv[KcbThanhtoanChitiet.Columns.PhuThu], 0);
            }
            m_dtReportPhieuThu.AcceptChanges();
            foreach (DataRow drv in m_dtReportPhieuThu.Rows)
            {
                drv["TONG_BN"] = Utility.Int32Dbnull(drv["SoLuong"], 0) *
                                   Utility.DecimaltoDbnull(drv["Discount_Price"], 0);
                drv["PHU_THU"] = Utility.Int32Dbnull(drv["SoLuong"], 0) *
                                              Utility.DecimaltoDbnull(drv[KcbThanhtoanChitiet.Columns.PhuThu], 0);
            }
            m_dtReportPhieuThu.AcceptChanges();
        }
        public DataTable KydongInphieuBaohiemChoBenhnhan(KcbThanhtoan objThanhtoan)
        {
            return null;
            //return SPs.BhytLaythongtinInphieubhKd(Utility.Int32Dbnull(objThanhtoan.IdThanhtoan, -1),
            //                                        Utility.sDbnull(objThanhtoan.MaLuotkham),
            //                                        Utility.Int32Dbnull(objThanhtoan.IdBenhnhan, -1)).GetDataSet().Tables[0];
        }
        public DataTable KydongInPhieubaohiemTraituyen(KcbThanhtoan objThanhtoan)
        {
            return null;
            //return SPs.BhytLaythongtinInphieuTraituyen(Utility.Int32Dbnull(objThanhtoan.IdThanhtoan, -1), Utility.sDbnull(objThanhtoan.MaLuotkham, ""),
            //                                      Utility.Int32Dbnull(objThanhtoan.IdBenhnhan, -1)).GetDataSet().Tables[0];
        }
        public DataTable DetmayPrintAllExtendExamPaymentDetail(KcbThanhtoan objThanhtoan)
        {
            return null;
            //return SPs.BhytLAYTHONGTInInphoibhytDm(Utility.Int32Dbnull(objThanhtoan.IdThanhtoan, -1),
            //                                              objThanhtoan.MaLuotkham,
            //                                              Utility.Int32Dbnull(objThanhtoan.IdBenhnhan, -1)).GetDataSet()
            //        .Tables[0];
        }
        public DataTable DetmayInphieuBhPhuthu(KcbThanhtoan objThanhtoan)
        {
            return null;
            //return SPs.BhytLAYTHONGTInInphoibhytPhuhuDm(Utility.Int32Dbnull(objThanhtoan.IdThanhtoan, -1),
            //                                              objThanhtoan.MaLuotkham,
            //                                              Utility.Int32Dbnull(objThanhtoan.IdBenhnhan, -1)).GetDataSet()
            //        .Tables[0];
        }
        public DataTable LaokhoaInbienlaiBhyt(KcbThanhtoan objThanhtoan)
        {
            return null;
            //return SPs.BhytInbienlai(Utility.Int32Dbnull(objThanhtoan.IdThanhtoan), Utility.sDbnull(objThanhtoan.MaLuotkham), Utility.Int32Dbnull(objThanhtoan.IdBenhnhan)).GetDataSet().Tables[0];
        }
        public DataTable LaokhoaInphieuBaohiemNgoaitru(KcbThanhtoan objThanhtoan)
        {
            return null;
            //return SPs.BhytLaythongtinInphoi(Utility.Int32Dbnull(objThanhtoan.IdThanhtoan, -1), Utility.sDbnull(objThanhtoan.MaLuotkham, ""),
            //                             Utility.Int32Dbnull(objThanhtoan.IdBenhnhan, -1), 0).GetDataSet().Tables[0];
        }

        public ActionResult UPDATE_SOBIENLAI(HoadonLog lHoadonLog)
        {
            try
            {
                using (var Scope = new TransactionScope())
                {
                    using (var dbScope = new SharedDbConnectionScope())
                    {
                        int record = -1;
                        record = new Update(HoadonLog.Schema).Set(HoadonLog.Columns.MauHoadon)
                            .EqualTo(lHoadonLog.MauHoadon).Set(HoadonLog.Columns.KiHieu).EqualTo(lHoadonLog.KiHieu)
                            .Set(HoadonLog.Columns.MaQuyen).EqualTo(lHoadonLog.MaQuyen)
                            .Set(HoadonLog.Columns.Serie).EqualTo(lHoadonLog.Serie)
                            .Where(HoadonLog.Columns.IdHdonLog).IsEqualTo(lHoadonLog.IdHdonLog)
                            .Execute();
                        if (record <= 0)
                        {
                            return ActionResult.Error;
                        }

                    }
                    Scope.Complete();
                    return ActionResult.Success;
                }

            }
            catch (Exception)
            {
                return ActionResult.Error;
            }
        }

        public ActionResult CHUYEN_DOITUONG(KcbLuotkham objLuotkham, string DOITUONG)
        {
            //try
            //{
            //    using (var Scope = new TransactionScope())
            //    {
            //        using (var dbScope = new SharedDbConnectionScope())
            //        {
            //            KcbDangkyKcbCollection TexamCollection =
            //                new Select().From(KcbDangkyKcb.Schema).Where(KcbDangkyKcb.Columns.MaLuotkham).IsEqualTo(
            //                    objLuotkham.MaLuotkham).And(KcbDangkyKcb.Columns.IdBenhnhan).IsEqualTo(objLuotkham.IdBenhnhan)
            //                    .ExecuteAsCollection<KcbDangkyKcbCollection>();
            //            if (TexamCollection.Count > 0)
            //            {
            //                //CHUYỂN GIÁ KHÁM BỆNH VÀO PHÒNG
            //                foreach (KcbDangkyKcb regExam in TexamCollection)
            //                {
            //                    if (Utility.Int32Dbnull(regExam.TrangthaiThanhtoan) == 1)
            //                    {
            //                        return ActionResult.ExistedRecord;
            //                    }
            //                    DmucDichvukcb KieuKhamCu = DmucDichvukcb.FetchByID(regExam.IdDichvuKcb);
            //                    DmucDichvukcb KieuKhamMoi =
            //                        new Select().From(DmucDichvukcb.Schema)
            //                        .Where(DmucDichvukcb.Columns.IdKieukham).IsEqualTo(KieuKhamCu.IdKieukham)
            //                        .And(DmucDichvukcb.Columns.IdKhoaphong).IsEqualTo(KieuKhamCu.IdKhoaphong)
            //                        .And(DmucDichvukcb.Columns.IdPhongkham).IsEqualTo(KieuKhamCu.IdPhongkham)
            //                        .And(DmucDichvukcb.Columns.MaDoituongKcb).IsEqualTo(DOITUONG)
            //                        .ExecuteSingle<DmucDichvukcb>();
            //                    regExam.IdDichvuKcb = Utility.Int16Dbnull(KieuKhamMoi.IdDichvukcb, -1);
            //                    if (objLuotkham.MaKhoaThuchien == "KYC")
            //                    {
            //                        regExam.DonGia = KieuKhamMoi.DonGia;
            //                        regExam.PhuThu = KieuKhamMoi.PhuthuDungtuyen;
            //                        regExam.Save();
            //                    }
            //                    else if (objLuotkham.MaKhoaThuchien == "KKB")
            //                    {
            //                        regExam.DonGia = KieuKhamMoi.DonGia;
            //                        if (Utility.sDbnull(objLuotkham.MaDoituongKcb, "DV") == "BHYT" && Utility.Int32Dbnull(objLuotkham.DungTuyen, 0) == 0)
            //                        {
            //                            regExam.PhuThu = KieuKhamMoi.PhuthuDungtuyen;
            //                        }
            //                        else
            //                        {
            //                            regExam.PhuThu = 0;
            //                        }
            //                        regExam.Save();

            //                        //THÊM CHI PHÍ DỊCH VỤ KÈM THEO KHÁM BỆNH
            //                        SqlQuery sql = new Select().From(KcbChidinhcl.Schema).Where(
            //                            KcbChidinhcl.Columns.MaLuotkham).
            //                            IsEqualTo(objLuotkham.MaLuotkham)
            //                            .And(KcbChidinhcl.Columns.IdBenhnhan).IsEqualTo(objLuotkham.IdBenhnhan);
            //                            //.And(KcbChidinhcl.Columns.IsPHIDvuKtheo).IsEqualTo(1);
            //                        int IdDV = -1;
            //                        string[] Ma_UuTien = globalVariables.gv_strMaUutien.Split(',');
            //                        if (!Ma_UuTien.Contains(Utility.sDbnull(objLuotkham.MaQuyenloi)))
            //                        {
            //                            if (Utility.Int32Dbnull(regExam.KhamNgoaigio, 0) == 1)
            //                            {
            //                                IdDV = Utility.Int32Dbnull(KieuKhamMoi.IdPhikemtheongoaigio, -1);
            //                            }
            //                            else
            //                            {
            //                                IdDV = Utility.Int32Dbnull(KieuKhamMoi.IdPhikemtheo, -1);
            //                            }
            //                        }
            //                        else
            //                        {
            //                            IdDV = -1;
            //                            KcbChidinhclCollection taCollection =
            //                                sql.ExecuteAsCollection<KcbChidinhclCollection>();
            //                            foreach (KcbChidinhcl assignInfo in taCollection)
            //                            {
            //                                KcbChidinhclsChitiet.Delete(KcbChidinhclsChitiet.Columns.IdChidinh, assignInfo.IdChidinh);
            //                                KcbChidinhcl.Delete(assignInfo.IdChidinh);
            //                            }
            //                        }
            //                        if (sql.GetRecordCount() <= 0)
            //                        {
            //                            //LServiceDetail lServiceDetail = LServiceDetail.FetchByID(IdDV);
            //                            //if (lServiceDetail != null)
            //                            //{
            //                            //    var objAssignInfo = new KcbChidinhcl();
            //                            //    objAssignInfo.ExamId = -1;
            //                            //    objAssignInfo.MaLuotkham = Utility.sDbnull(objLuotkham.MaLuotkham, "");
            //                            //    objAssignInfo.IdBenhnhan = Utility.Int32Dbnull(objLuotkham.IdBenhnhan, "");

            //                            //    objAssignInfo.RegDate = globalVariables.SysDate;
            //                            //    objAssignInfo.DepartmentId = globalVariables.DepartmentID;
            //                            //    objAssignInfo.TrangthaiThanhtoan = 0;
            //                            //    objAssignInfo.CreatedBy = globalVariables.UserName;
            //                            //    objAssignInfo.CreateDate = globalVariables.SysDate;
            //                            //    objAssignInfo.Actived = 0;
            //                            //    objAssignInfo.MaKhoaThien = globalVariables.MA_KHOA_THIEN;
            //                            //    objAssignInfo.NoiTru = 0;
            //                            //    objAssignInfo.IdDoituongKcb = Utility.Int16Dbnull(
            //                            //        objLuotkham.IdDoituongKcb, -1);
            //                            //    objAssignInfo.DiagPerson = globalVariables.gv_intIDNhanvien;
            //                            //    objAssignInfo.DepartmentId = globalVariables.DepartmentID;
            //                            //    objAssignInfo.IsPHIDvuKtheo = 1;
            //                            //    objAssignInfo.IsNew = true;
            //                            //    objAssignInfo.Save();
            //                            //    objAssignInfo.IdChidinh =
            //                            //        Utility.Int32Dbnull(
            //                            //            KcbChidinhcl.CreateQuery().GetMax(KcbChidinhcl.Columns.IdChidinh), -1);
            //                            //    var objAssignDetail = new KcbChidinhclsChitiet();
            //                            //    objAssignDetail.ExamId = -1;
            //                            //    objAssignDetail.IdChidinh = objAssignInfo.IdChidinh;
            //                            //    objAssignDetail.ServiceId = lServiceDetail.ServiceId;
            //                            //    objAssignDetail.IdDichvuChitiet = lServiceDetail.IdDichvuChitiet;
            //                            //    objAssignDetail.DiscountPrice = 0;
            //                            //    objAssignDetail.DiscountRate = 0;
            //                            //    objAssignDetail.DiscountType = 0;
            //                            //    objAssignDetail.DonGia = Utility.DecimaltoDbnull(lServiceDetail.Price,
            //                            //                                                          0);
            //                            //    objAssignDetail.DiscountPrice = Utility.DecimaltoDbnull(
            //                            //        lServiceDetail.Price, 0);
            //                            //    objAssignDetail.PhuThu = 0;
            //                            //    objAssignDetail.UserId = globalVariables.UserName;
            //                            //    objAssignDetail.AssignTypeId = 0;
            //                            //    objAssignDetail.InputDate = globalVariables.SysDate;
            //                            //    objAssignDetail.TrangthaiThanhtoan = 0;
            //                            //    objAssignDetail.TuTuc = (byte?)(Utility.sDbnull(objLuotkham.MaDoituongKcb) == "DV" ? 0 : 1);
            //                            //    objAssignDetail.SoLuong = 1;
            //                            //    objAssignDetail.AssignDetailStatus = 0;
            //                            //    objAssignDetail.SDesc =
            //                            //        "Chi phí đi kèm thêm phòng khám khi đăng ký khám bệnh theo yêu cầu";
            //                            //    objAssignDetail.BhytStatus = 0;
            //                            //    objAssignDetail.DisplayOnReport = 1;
            //                            //    objAssignDetail.GiaBhytCt = 0;
            //                            //    objAssignDetail.GiaBnct = Utility.DecimaltoDbnull(lServiceDetail.Price, 0);
            //                            //    objAssignDetail.IpMayTao = globalVariables.IpAddress;
            //                            //    objAssignDetail.IpMacTao = globalVariables.IpMacAddress;
            //                            //    objAssignDetail.ChoPhepIn = 0;
            //                            //    objAssignDetail.AssignDetailStatus = 0;
            //                            //    objAssignDetail.DiagPerson = globalVariables.gv_intIDNhanvien;
            //                            //    objAssignDetail.IdDoituongKcb =
            //                            //        Utility.Int16Dbnull(objLuotkham.IdDoituongKcb,
            //                            //                            -1);
            //                            //    objAssignDetail.Stt = 1;
            //                            //    objAssignDetail.IsNew = true;
            //                            //    objAssignDetail.Save();
            //                            //}
            //                        }
            //                    }


            //                }
            //                //CHUYỂN GIÁ DỊCH VỤ CẬN LÂM SÀNG
            //                KcbChidinhclCollection taAssignInfoCollection = new Select().From(KcbChidinhcl.Schema).
            //                    Where(KcbChidinhcl.Columns.MaLuotkham).IsEqualTo(objLuotkham.MaLuotkham)
            //                    .And(KcbChidinhcl.Columns.IdBenhnhan).IsEqualTo(objLuotkham.IdBenhnhan)
            //                    .And(KcbChidinhcl.Columns.IsPHIDvuKtheo).IsEqualTo(0)
            //                    .ExecuteAsCollection<KcbChidinhclCollection>();
            //                foreach (KcbChidinhcl assignInfo in taAssignInfoCollection)
            //                {
            //                    KcbChidinhclsChitietCollection tAssignDetailCollection =
            //                        new Select().From(KcbChidinhclsChitiet.Schema)
            //                            .Where(KcbChidinhclsChitiet.Columns.IdChidinh).IsEqualTo(assignInfo.IdChidinh).
            //                            ExecuteAsCollection<KcbChidinhclsChitietCollection>();
            //                    foreach (KcbChidinhclsChitiet assignDetail in tAssignDetailCollection)
            //                    {
            //                        if (Utility.Int32Dbnull(assignDetail.TrangthaiThanhtoan) == 1)
            //                        {
            //                            return ActionResult.ExistedRecord;
            //                        }
            //                        DmucDoituongkcbService lObjectTypeService = new Select().From(DmucDoituongkcbService.Schema)
            //                            .Where(DmucDoituongkcbService.Columns.IdDichvuChitiet).IsEqualTo(assignDetail.IdDichvuChitiet)
            //                            .And(DmucDoituongkcbService.Columns.MaDtuong).IsEqualTo(objLuotkham.MaDoituongKcb)
            //                            .And(DmucDoituongkcbService.Columns.MaKhoaThien).IsEqualTo(objLuotkham.MaKhoaThien).ExecuteSingle<DmucDoituongkcbService>();
            //                        if (lObjectTypeService != null)
            //                        {
            //                            assignDetail.DiscountPrice = Utility.DecimaltoDbnull(lObjectTypeService.LastPrice, 0);
            //                            if (Utility.sDbnull(objLuotkham.MaDoituongKcb, "DV") == "BHYT" && Utility.Int32Dbnull(objLuotkham.DungTuyen, 0) == 0)
            //                            {
            //                                assignDetail.PhuThu = Utility.DecimaltoDbnull(lObjectTypeService.PhuThuTraiTuyen, 0);
            //                            }
            //                            else
            //                            {
            //                                assignDetail.PhuThu = Utility.DecimaltoDbnull(lObjectTypeService.Surcharge, 0);
            //                            }
            //                        }
            //                        else
            //                        {
            //                            if (Utility.sDbnull(objLuotkham.MaDoituongKcb) == "BHYT")
            //                            {

            //                                DmucDoituongkcbService lObjectTypeServiceDv = new Select().From(DmucDoituongkcbService.Schema)
            //                                    .Where(DmucDoituongkcbService.Columns.IdDichvuChitiet).IsEqualTo(assignDetail.IdDichvuChitiet)
            //                                    .And(DmucDoituongkcbService.Columns.MaDtuong).IsEqualTo("DV").And(DmucDoituongkcbService.Columns.MaKhoaThien).IsEqualTo(objLuotkham.MaKhoaThien).ExecuteSingle<DmucDoituongkcbService>();
            //                                if (lObjectTypeServiceDv != null)
            //                                {
            //                                    assignDetail.DiscountPrice = Utility.DecimaltoDbnull(lObjectTypeServiceDv.LastPrice, 0);
            //                                    assignDetail.PhuThu = 0;
            //                                    assignDetail.TuTuc = 1;
            //                                }
            //                                else
            //                                {
            //                                    Utility.ShowMsg("Không có giá Dịch Vụ");
            //                                    return ActionResult.Exceed;
            //                                }
            //                            }
            //                        }
            //                        assignDetail.Save();
            //                    }
            //                }
            //                objLuotkham.Save();
            //            }
            //        }
            //        Scope.Complete();
            //    }
            //    return ActionResult.Success;
            //}
            //catch (Exception)
            //{
                return ActionResult.Error;
            //}
        }
        public ActionResult Chotbaocao(DateTime NgayChot, DateTime ngayThanhToan)
        {
            try
            {
                using (var Scope = new TransactionScope())
                {
                    using (var dbScope = new SharedDbConnectionScope())
                    {

                        SPs.KcbThanhtoanChot(NgayChot.ToString("dd/MM/yyyy"), ngayThanhToan.ToString("dd/MM/yyyy")).Execute();
                    }
                    Scope.Complete();
                    return ActionResult.Success;
                }
            }
            catch (Exception)
            {
                return ActionResult.Error;
            }
        }
        public ActionResult ChotVetbaocao(DateTime NgayChot, DateTime NgayThanhToan)
        {
            try
            {
                using (var Scope = new TransactionScope())
                {
                    using (var dbScope = new SharedDbConnectionScope())
                    {
                        SPs.KcbThanhtoanChotvet(NgayChot.ToString("dd/MM/yyyy"), NgayThanhToan.ToString("dd/MM/yyyy")).Execute();
                    }
                    Scope.Complete();
                    return ActionResult.Success;
                }
            }
            catch (Exception)
            {
                return ActionResult.Error;
            }
        }
       


    }

}
