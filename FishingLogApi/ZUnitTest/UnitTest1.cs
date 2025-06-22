using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FishingLogApi.DAL.Models;
using FishingLogApi.DAL.Repositories;

namespace FishingLogApi.DAL;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestMethod1()
    {
        Repositories.VeidistadurRepository veidistadirRepo = new VeidistadurRepository();
        Repositories.VeidiferdirRepository veidiferdirRepo = new VeidiferdirRepository();
        FishingLogApi.DAL.Repositories.VeidiferdirRepository repo = new FishingLogApi.DAL.Repositories.VeidiferdirRepository();
        var veidiferd = new Veidiferd();
        veidiferd.DagsFra = new DateTime(2089, 3, 2);
        veidiferd.DagsTil = new DateTime(2089, 3, 5);
        veidiferd.KoId = "-1";
        veidiferd.VsId = "-1";
        veidiferd.VetId = "-1";
        veidiferd.Lysing = "bara test";
        //veidiferd.Ar = "2099";
        veidiferd.Id = "482";
        repo.UpdateVeidiferd(veidiferd);
        //repo.AddVeidiferd(veidiferd);


        //var res = veidiferdirRepo.IdExists("465");
        //var listVeidiferdir = veidiferdirRepo.GetVeidiferdir();

        //var list = veidistadirRepo.GetVeidistadir();
    }

    //[TestMethod]
    //public void TestMethod1()
    //{
    //    FishingLogApi.DAL.VeidiferdirRepository repo = new FishingLogApi.DAL.VeidiferdirRepository();
    //    var veidiferd = new Veidiferd();
    //    veidiferd.DagsFra = new DateTime(2099, 3, 2);
    //    veidiferd.DagsTil = new DateTime(2099, 3, 5);
    //    veidiferd.KoId = "-1";
    //    veidiferd.vsId = "-1";
    //    veidiferd.VetId = "-1";
    //    veidiferd.Lysing = "test færsla";
    //    veidiferd.Ar = "2099";
    //    repo.AddVeidiferd(veidiferd);
    //}
}
