using System;
using System.Collections.Generic;
using iChronoMe.Core.Types;

namespace iChronoMe.Core.DynamicCalendar
{
    public static class DynamicColors
    {               
        private static void StepCheck(Int32 numOfSteps, ref Int32 step)
        {
            if (step > numOfSteps)
                step -= numOfSteps;
            else if (step < 0)
                step += numOfSteps;
        }

        private static void StepCheck(double numOfSteps, ref double step)
        {
            if (step > numOfSteps)
                step -= numOfSteps;
            else if (step < 0)
                step += numOfSteps;
        }

        public static xColor RainbowColor(Int32 numOfSteps, Int32 step)
        {
            StepCheck(numOfSteps, ref step);
            return xColor.FromHsla(1.0 / numOfSteps * step, 0.5, 0.5);
        }

        static Random rnd = null;
        public static xColor RandomColor()
        {
            if (rnd == null)
                rnd = new Random((int)-(DateTime.Now.TimeOfDay.TotalMilliseconds));
            return xColor.FromRgba(rnd.Next(200), rnd.Next(200), rnd.Next(200), 255);
        }

        public static xColor[] EarthColors = new xColor[] {
                xColor.FromRgba(137, 124, 89, 255),
                xColor.FromRgba(203, 165, 103, 255),
                xColor.FromRgba(194, 73, 56, 255),
                xColor.FromRgba(233, 148, 21, 255),
                xColor.FromRgba(81, 56, 49, 255),
                xColor.FromRgba(195, 174, 147, 255),
                xColor.FromRgba(135, 39, 41, 255),
                xColor.FromRgba(169, 138, 110, 255),
                xColor.FromRgba(125, 163, 172, 255)
            };

        public static List<string[]> SampleColorSetS { get; } = new List<string[]>
            {
                new string[] { "#FF1F2D3B", "#FF2D5D6C", "#FF7F8CA5", "#FFF9F9F8", "#FFF99709" },
                new string[] { "#FF9A3841", "#FFD8A374", "#FFF1D8A6", "#FFAEC8A5", "#FF71A794" },
                new string[] { "#FF4F393F", "#FF637464", "#FFC5C6AC", "#FFF9FBFA", "#FF0F7375" },
                new string[] { "#FF52151B", "#FF6B1E08", "#FFEF5D07", "#FFE6B038", "#FF83A54B" },
                new string[] { "#FF75BEA7", "#FFD5DB82", "#FFF59F35", "#FFE45932", "#FF914E2A" },
                new string[] { "#FF466262", "#FFA4B97A", "#FFF0B034", "#FFF18E35", "#FFF05A36" },
                new string[] { "#FFA2C96F", "#FFEFC912", "#FFF2A110", "#FFE63A23", "#FF5F2D4A" },
                new string[] { "#FF492928", "#FFD55D27", "#FFF2730D", "#FFF7EEE1", "#FF5D8192" },
                new string[] { "#FFFC892E", "#FFD3D358", "#FF569582", "#FF45735C", "#FF1E4145" },
                new string[] { "#FF0C2943", "#FF0B3845", "#FF689D6A", "#FFD0EE9D", "#FF496C5C" },
                new string[] { "#FF413E40", "#FFF1F2F0", "#FFB5CEAA", "#FF769325", "#FF395618" },
                new string[] { "#FF53C3BF", "#FF9AE8A8", "#FFF3D857", "#FFE86740", "#FFF01B42" },
                new string[] { "#FFB62637", "#FFE95A11", "#FFF7F4E8", "#FF91BAB0", "#FF405C63" },
                new string[] { "#FF461426", "#FF8C1C33", "#FFBD675A", "#FFDB9170", "#FFB29752" },
                new string[] { "#FF1A1C1C", "#FF28432F", "#FF929B62", "#FFCAD69C", "#FFD4E9D2" },
                new string[] { "#FFA93433", "#FFD5A349", "#FFFAF1B8", "#FF8DB678", "#FF41472C" },
                new string[] { "#FF8B9868", "#FFCDA06C", "#FFC69367", "#FFC4885A", "#FF2E3738" },
                new string[] { "#FFC3B27C", "#FFEFCF96", "#FFB8B285", "#FF767564", "#FF5A4545" },
                new string[] { "#FF1D2A2F", "#FF3C5458", "#FF598990", "#FF6BDDD2", "#FFEEF7E6" },
                new string[] { "#FF2E4659", "#FF85AAA9", "#FFF7F0DF", "#FFDE4A1D", "#FFCE2835" },
                new string[] { "#FF3D2F3D", "#FFC08747", "#FFFDD68A", "#FFC4D69C", "#FF3C8B6E" },
                new string[] { "#FF4A9574", "#FFA5AD8F", "#FFE28646", "#FFE5157F", "#FFBE2B53" },
                new string[] { "#FF673320", "#FFCB5C0A", "#FFB70D0D", "#FF711111", "#FF4D0F0D" },
                new string[] { "#FFF2ECC2", "#FFA0B695", "#FF7C735D", "#FF6B5954", "#FF4E4644" },
                new string[] { "#FF55524A", "#FF878369", "#FFAEB283", "#FFC1BE8B", "#FFA08964" },
                new string[] { "#FF97AF63", "#FF7C9B62", "#FF567B78", "#FF3E5E47", "#FF404A35" },
                new string[] { "#FFF6AD8F", "#FFDE987C", "#FFCDB5A1", "#FFB8E4BA", "#FFB0C6B8" },
                new string[] { "#FF083E48", "#FF095857", "#FF419288", "#FFBFCDA5", "#FFF1DB4E" },
                new string[] { "#FFFBFAEE", "#FFE0E0CC", "#FF75A67A", "#FF523327", "#FF461012" },
                new string[] { "#FFF35879", "#FFB66876", "#FFC2BC90", "#FFABD697", "#FF95F3BF" },
                new string[] { "#FFE85A2F", "#FFF9B322", "#FFAAE27B", "#FF1A7A86", "#FF0B4344" },
                new string[] { "#FFBC8B75", "#FFAEBAA9", "#FF639DB4", "#FF3A61B6", "#FF223F70" },
                new string[] { "#FFF7F7F2", "#FF95BA8E", "#FF336679", "#FF222E31", "#FF201C28" },
                new string[] { "#FF798D64", "#FF78607D", "#FF555C50", "#FF685D4F", "#FFCAA74C" },
                new string[] { "#FF0D1A28", "#FF0A2040", "#FF353C60", "#FF555955", "#FFE1C079" },
                new string[] { "#FF57A99E", "#FFB5AA91", "#FFF4C466", "#FF94B121", "#FF819824" },
                new string[] { "#FF174138", "#FF5FAE45", "#FFEFA81A", "#FFFA6810", "#FFC40F11" },
                new string[] { "#FFA30E10", "#FFFB9C12", "#FFE3B522", "#FF5E8556", "#FF1B5552" },
                new string[] { "#FF957C42", "#FFD88254", "#FF9A241E", "#FF3A0F10", "#FF3C0C1C" },
                new string[] { "#FF242F43", "#FF5A5756", "#FF889874", "#FFD3E0B5", "#FFBFE9CA" },
                new string[] { "#FF414F4E", "#FF9E907D", "#FFE1C696", "#FFD2DFB1", "#FF9FCC6F" },
                new string[] { "#FF4A2E38", "#FF502C44", "#FF664B3D", "#FFAE9776", "#FFE6CCC1" },
                new string[] { "#FFFC8E07", "#FFDAB022", "#FF608007", "#FF28613E", "#FF2A563B" },
                new string[] { "#FFEEF4F5", "#FF1A5174", "#FF2D6680", "#FF39555C", "#FF4D5452" },
                new string[] { "#FFD2C13B", "#FFEE890E", "#FFB13C13", "#FF851315", "#FF400A0C" },
                new string[] { "#FF213B50", "#FF237E74", "#FF9DC8A0", "#FFB39F60", "#FFE06937" },
                new string[] { "#FFF2F2E6", "#FFC6BB96", "#FF64644D", "#FF3A4D53", "#FF93A2AF" },
                new string[] { "#FF324E62", "#FF1F7EA6", "#FF5AC1CE", "#FFB6DA71", "#FFFEA512" },
                new string[] { "#FF80D8DD", "#FF388B89", "#FF27533D", "#FFCA912C", "#FFF1EC95" },
                new string[] { "#FFE16C31", "#FFC06558", "#FF6F97BC", "#FFB0C4EB", "#FFBFF8F8" },
                new string[] { "#FF4C4342", "#FF736C6D", "#FFAE9175", "#FFE8DFB8", "#FFC8C69C" },
                new string[] { "#FFC6C68C", "#FFB5D6B3", "#FF51B3AE", "#FF4C452F", "#FF4D3C3A" },
                new string[] { "#FF575247", "#FF96655A", "#FFBDA66D", "#FFCBDF8E", "#FFCAEDBA" },
                new string[] { "#FFADB457", "#FFF2D182", "#FFBB853B", "#FF8F3A1D", "#FF46110E" },
                new string[] { "#FFBA405F", "#FFFD984F", "#FFE8D88F", "#FF7DBA5A", "#FFFAF764" },
                new string[] { "#FF0CA59D", "#FF1FC188", "#FFC8B648", "#FFC3763C", "#FFBE4254" },
                new string[] { "#FF323D3D", "#FF56BFB3", "#FFF0F0D0", "#FFEEC163", "#FFDD653D" },
                new string[] { "#FF172D50", "#FF0F383D", "#FFB01D26", "#FFD83718", "#FFEECD54" },
                new string[] { "#FF0B2142", "#FF216F91", "#FFFAF9F2", "#FFC0A081", "#FFA43B28" },
                new string[] { "#FFED7250", "#FFCE944E", "#FF3E3F44", "#FF1A7285", "#FF066291" },
                new string[] { "#FFEEB14D", "#FFB97A41", "#FF932B1C", "#FF462815", "#FF856636" },
                new string[] { "#FF1D1930", "#FF1B3F5A", "#FF77686E", "#FFDBAFA7", "#FFDC8E36" },
                new string[] { "#FF9D6E66", "#FFF5DEA7", "#FFD7A564", "#FFBF3F26", "#FF6F3B29" },
                new string[] { "#FF0B1D1C", "#FF31575A", "#FF9CBFAA", "#FFD8D6C0", "#FFC8C69B" },
                new string[] { "#FFF53A60", "#FFF2BF34", "#FFF7E045", "#FF9EE456", "#FF22AF77" },
                new string[] { "#FF294D47", "#FF49A3AB", "#FFF2F3EC", "#FFDD4D37", "#FFAA2734" },
                new string[] { "#FFCB7473", "#FFF1F1EC", "#FFD7D7D6", "#FF8E8D8F", "#FF2A5076" },
                new string[] { "#FFD9CF81", "#FF9DC391", "#FF4A7A64", "#FF30655D", "#FF1D4E51" },
                new string[] { "#FF151D3A", "#FF233F49", "#FF6A979E", "#FF83AEED", "#FFDBEAEC" },
                new string[] { "#FFE54333", "#FFE8A43E", "#FFE9E6B2", "#FF4CC7EF", "#FF418684" },
                new string[] { "#FF422442", "#FF997A71", "#FFFCA54B", "#FFEF852B", "#FFA68874" },
                new string[] { "#FF3C2029", "#FF75523C", "#FFC08F5E", "#FFF5ECA1", "#FFADCA92" },
                new string[] { "#FFC2896E", "#FFD5B6AF", "#FFB1CDCC", "#FFC1EFEB", "#FFFADFD8" },
                new string[] { "#FF413C3F", "#FFF8F6F2", "#FFC3BEC4", "#FFA0ABBD", "#FF3B6679" },
                new string[] { "#FFAB7324", "#FFC02D51", "#FF8937E0", "#FF107DEF", "#FF47BFCC" },
                new string[] { "#FFDFA848", "#FF303713", "#FF7D7525", "#FF90C29B", "#FF3B9AAB" },
                new string[] { "#FF853E4F", "#FFDF6224", "#FFE2D769", "#FF2EA57A", "#FF1D3E20" },
                new string[] { "#FFF5F5F3", "#FFD0BFB2", "#FF99A397", "#FF888A8D", "#FF4C5858" },
                new string[] { "#FF132729", "#FF748B48", "#FFF78E31", "#FFE66E2A", "#FFAA9F63" },
                new string[] { "#FF3D161D", "#FF841011", "#FFB24919", "#FFADA640", "#FFFBFAF2" },
                new string[] { "#FF8CC228", "#FFF97016", "#FFD1471C", "#FFB51D0D", "#FF660D18" },
                new string[] { "#FF1A1924", "#FF532734", "#FF8E594E", "#FFEAB25C", "#FFF6D5A3" },
                new string[] { "#FF253231", "#FF4D6253", "#FF8F7F61", "#FFCBAA73", "#FFFCF9F2" },
                new string[] { "#FF64BC9C", "#FFE9DF6B", "#FFEBB238", "#FFF37125", "#FFCC3D48" },
                new string[] { "#FFDE1277", "#FFED3B60", "#FFD9A85F", "#FFAED850", "#FFC0D70E" },
                new string[] { "#FF418BA3", "#FFF4B3AB", "#FFF2D3B5", "#FFC7C99D", "#FF7DD5B9" },
                new string[] { "#FFACAA6E", "#FF306F74", "#FF093054", "#FF680A0A", "#FF86272E" },
                new string[] { "#FF6DE8CE", "#FF5BCDB5", "#FFDBBE79", "#FF95564F", "#FF2C2D2F" },
                new string[] { "#FF092136", "#FF33402C", "#FFBDCA86", "#FF38ACC5", "#FF31A1F5" },
                new string[] { "#FF362A31", "#FF818D45", "#FFC2B84E", "#FFECD87E", "#FFDC974F" },
                new string[] { "#FF4B849C", "#FF448DA8", "#FF698CB0", "#FF4D8196", "#FF3F282A" },
                new string[] { "#FFBD3C52", "#FFDEA32B", "#FFF7CC43", "#FF34B879", "#FF259C7E" },
                new string[] { "#FF25392D", "#FF26373A", "#FF515460", "#FF495E67", "#FF926F89" },
                new string[] { "#FFEA813A", "#FFE9E4B6", "#FFAE986B", "#FF56696B", "#FF362F47" },
                new string[] { "#FF19272E", "#FFD9B522", "#FFCC4930", "#FF7C0B0D", "#FF420608" },
                new string[] { "#FFE8AD39", "#FF912417", "#FFBE3910", "#FF77A273", "#FF81D6D5" },
                new string[] { "#FFED3C3C", "#FF16252E", "#FF77B59D", "#FFAFCEBC", "#FFDFC57B" },
                new string[] { "#FF3C313A", "#FFA77D7C", "#FFF8F3EA", "#FFC7AF9B", "#FFB14848" },
                new string[] { "#FF651F46", "#FFC8495C", "#FFEF8420", "#FFF41712", "#FF1A9E9C" },
                new string[] { "#FF369FAC", "#FF47C7B5", "#FFEBDBBA", "#FFFE9BA2", "#FFFB5578" },
                new string[] { "#FF1D6869", "#FF349D7C", "#FF80B94B", "#FFBEEE0D", "#FFE9F6E9" },
                new string[] { "#FF0F35F1", "#FF166C86", "#FF162245", "#FF3A3C2F", "#FFD7CD35" },
                new string[] { "#FFEBDAD2", "#FFCBDAE5", "#FFADC9DC", "#FF385A83", "#FF223158" },
                new string[] { "#FF1FBDA3", "#FFEE7855", "#FFF8CA8C", "#FFFC2F31", "#FF9E0B0A" },
                new string[] { "#FF985224", "#FFAE735A", "#FF7EA8B9", "#FFB9E1F1", "#FFF5FBFC" },
                new string[] { "#FF1F3550", "#FF22988C", "#FFABD5AD", "#FFEBDCBA", "#FFE4655B" },
                new string[] { "#FF366278", "#FF33AFAE", "#FFF9EFA7", "#FFDDB951", "#FFFA5261" },
                new string[] { "#FFB9A48B", "#FF478182", "#FF279D7F", "#FFE65331", "#FFEA3A3F" },
                new string[] { "#FF0F283A", "#FF62834B", "#FFFB841E", "#FFE26426", "#FFADB35C" },
                new string[] { "#FF284246", "#FF3E716B", "#FFB4E2B7", "#FFFDEBA5", "#FFE44D39" },
                new string[] { "#FFC4F75F", "#FFD4C526", "#FF7F6F61", "#FF524A40", "#FF332E29" },
                new string[] { "#FF262946", "#FF4772B0", "#FF5DADAD", "#FFCAC999", "#FFE09533" },
                new string[] { "#FF574A36", "#FF87C140", "#FFDBB520", "#FFEF9F10", "#FFE23131" },
                new string[] { "#FF18ACDA", "#FF2FC8A2", "#FFCBB75F", "#FFBF7344", "#FFB94D57" },
                new string[] { "#FF7D325E", "#FFF9B7C4", "#FFDFBFB1", "#FF8E9C95", "#FF424447" },
                new string[] { "#FFD21C19", "#FFC43B16", "#FFE59B30", "#FF3D8F6F", "#FF162B58" },
                new string[] { "#FFA4295E", "#FF621B27", "#FF96855B", "#FF90B7A9", "#FFD8DDB9" },
                new string[] { "#FF45585D", "#FF644D52", "#FFF27A52", "#FFF8994E", "#FFEADB97" },
                new string[] { "#FF517072", "#FF759F8D", "#FFC7B89E", "#FFE5A580", "#FFDE6365" },
                new string[] { "#FFFC0A0C", "#FFFB4309", "#FFF6C11A", "#FF259F48", "#FF122E34" },
                new string[] { "#FF193A3C", "#FF1C5D6A", "#FF958B5A", "#FFE2BB68", "#FFF8E251" },
                new string[] { "#FF434D53", "#FFA9B9A1", "#FFF4E4AB", "#FFE58E7F", "#FFCA4D5D" },
                new string[] { "#FF3D6663", "#FF81A68B", "#FFEDD7B8", "#FFD1B786", "#FFA75E54" },
                new string[] { "#FFCFDDCD", "#FFB8B5AE", "#FF84999A", "#FF393634", "#FF252F20" },
                new string[] { "#FFA8484E", "#FFCDCFD0", "#FFC2BDA4", "#FF99CD64", "#FFC3C5A6" },
                new string[] { "#FFE9D3B7", "#FFC0C086", "#FF6B8085", "#FF5D567E", "#FF422A40" },
                new string[] { "#FF515776", "#FFD32382", "#FFF7469F", "#FFE73393", "#FFD0A69C" },
                new string[] { "#FF497C72", "#FF93C59F", "#FFF4CE9A", "#FFE6413F", "#FFB9262B" },
                new string[] { "#FF108F95", "#FFF88C6A", "#FFFBE3A1", "#FFDEB78C", "#FFA0856D" },
                new string[] { "#FF4E5752", "#FFD47753", "#FFDCD290", "#FFB5AB76", "#FF696547" },
                new string[] { "#FF4799B2", "#FF7AB6BB", "#FFC0BEB0", "#FFF9EBD7", "#FFF56E91" },
                new string[] { "#FF2E4849", "#FF87BD29", "#FFFEF21A", "#FFBD7A74", "#FF295B9A" },
                new string[] { "#FF5FE9D4", "#FFD3DBBC", "#FFEA7570", "#FFE3AE41", "#FFBCE386" },
                new string[] { "#FF0C2E37", "#FF133F3B", "#FF468168", "#FFB1C866", "#FFECEFA2" },
                new string[] { "#FF2B3D39", "#FFF1E19E", "#FFD9C95A", "#FFA68133", "#FF6C2E24" },
                new string[] { "#FF925013", "#FFE9B43F", "#FFC06F47", "#FFB7A03A", "#FFC15A06" },
                new string[] { "#FF5A5452", "#FFB7B792", "#FFB59C85", "#FFBDA97E", "#FF7F5950" },
                new string[] { "#FFF59E19", "#FFCF501E", "#FFC5372A", "#FFA74B33", "#FF7E8A8D" },
                new string[] { "#FFDCBC7E", "#FFEF4D5B", "#FFDA0B27", "#FF8A3A4B", "#FF447965" },
                new string[] { "#FF2C6B7D", "#FF4DA9AB", "#FFCBDDA9", "#FFE2B13E", "#FFF9BD15" },
                new string[] { "#FFC63733", "#FFC05C29", "#FFA7B92B", "#FF139E5E", "#FF115413" },
                new string[] { "#FF5B293B", "#FF8C3947", "#FFB9892B", "#FFDD8F40", "#FFEEED8C" },
                new string[] { "#FF974468", "#FFD79CA4", "#FFBEB1A0", "#FFAACFD3", "#FFE7F1E3" },
                new string[] { "#FF22252A", "#FF3F7B3A", "#FF30543F", "#FFF4E8BE", "#FFDB8C56" },
                new string[] { "#FF455B47", "#FFD4CF9A", "#FFFCC36A", "#FFA7503B", "#FF48262E" },
                new string[] { "#FF393C3F", "#FF4D7966", "#FFA59462", "#FFCDBA70", "#FFF6F4EB" },
                new string[] { "#FF117691", "#FF34C1C2", "#FFF1ECBF", "#FFEE9338", "#FFE12A37" },
                new string[] { "#FF233530", "#FFB7EBF0", "#FFFFFEFC", "#FF28292F", "#FF1E1619" },
                new string[] { "#FF131D34", "#FF35425D", "#FF58546F", "#FFC6A7A8", "#FFAF9489" },
                new string[] { "#FF602A33", "#FFAD874B", "#FFB87E48", "#FFC0BB77", "#FFF3D598" },
                new string[] { "#FF136870", "#FF5793AC", "#FFEAD2BF", "#FF969A9D", "#FF545156" },
                new string[] { "#FF33505E", "#FF44B6A4", "#FFAAD0C5", "#FFD1C970", "#FFDA6324" },
                new string[] { "#FF0E2333", "#FF28686F", "#FFE4B96F", "#FFD64F20", "#FF75101B" },
                new string[] { "#FF443832", "#FF808461", "#FFA8976F", "#FFE6CB9C", "#FF953326" },
                new string[] { "#FF2F4444", "#FF393439", "#FF4A5B5F", "#FFCAB492", "#FFFAECD0" },
                new string[] { "#FF559899", "#FFCAC6E7", "#FFE3E1CD", "#FFF8BF8A", "#FFDD847A" },
                new string[] { "#FF1D2E4D", "#FF2F8F9E", "#FF73CCB7", "#FFD3E7CD", "#FFFCFACC" },
                new string[] { "#FFF0E4B4", "#FFD2EE97", "#FFA9DA9F", "#FFBDC4B7", "#FF8EA9B5" },
                new string[] { "#FF537584", "#FFABB1A9", "#FFF7B5A7", "#FFECC1A1", "#FFC2A68A" },
                new string[] { "#FF4F3343", "#FFB3925E", "#FFAB8F51", "#FFD19F6E", "#FF8E7156" },
                new string[] { "#FF354843", "#FFBB747B", "#FFE7BEB9", "#FFE2D89E", "#FF8B6B49" },
                new string[] { "#FF132523", "#FF323538", "#FFD78E38", "#FFF7F0C4", "#FFB2CE7A" },
                new string[] { "#FF478271", "#FF96E6C1", "#FFFBCC50", "#FFFC8A3B", "#FFC85758" },
                new string[] { "#FF37707D", "#FFA9867D", "#FFE1AA64", "#FFF1D2B1", "#FFFD496D" },
                new string[] { "#FF42413A", "#FFBE9856", "#FFC74419", "#FF950F10", "#FF7F101F" },
                new string[] { "#FF903D1E", "#FFA9602A", "#FFD3AB6F", "#FF06A5F5", "#FF45D0DD" },
                new string[] { "#FFC68A48", "#FFF4EBB3", "#FFA4623C", "#FF501739", "#FF2E1C2D" },
                new string[] { "#FFDD3E48", "#FFFFA753", "#FFCBCA6A", "#FF1E9C98", "#FF0D474F" },
                new string[] { "#FFFFFEFD", "#FF92DFED", "#FF6ECBAA", "#FF333330", "#FF32231B" },
                new string[] { "#FFF82353", "#FF878E6E", "#FFE5B953", "#FFE8C53E", "#FFCEC76E" },
                new string[] { "#FF2A3A30", "#FFADF6D1", "#FFF6E6CE", "#FFD88992", "#FF273C41" },
                new string[] { "#FF30404F", "#FFFC444C", "#FFEDCD5A", "#FF4FBBAF", "#FF256C75" },
                new string[] { "#FF242226", "#FF528D74", "#FFCECFBA", "#FFC1AF88", "#FFD07043" },
                new string[] { "#FF272835", "#FF362F58", "#FF0899E0", "#FFA0C2D3", "#FFF7F9E6" },
                new string[] { "#FF2F2D2F", "#FF415249", "#FF4F784F", "#FF99C65C", "#FFE0BC29" },
                new string[] { "#FF31E4B2", "#FF97DFA7", "#FFFCF2BC", "#FFF09583", "#FFAB4875" },
                new string[] { "#FF111E36", "#FF23456D", "#FF4B6493", "#FFC5BABC", "#FFB9A38D" },
                new string[] { "#FFF8102A", "#FFFC10F6", "#FF9CA9C6", "#FF397A64", "#FF589E40" },
                new string[] { "#FF13273E", "#FF224951", "#FF436D61", "#FFE5957E", "#FFEFEDCF" },
                new string[] { "#FF3B2246", "#FFF9234D", "#FFFCDEC0", "#FF0B8F7B", "#FF2FB162" },
                new string[] { "#FF5C4853", "#FF914374", "#FFE42D5B", "#FFDA683E", "#FFF6D3B1" },
                new string[] { "#FF314439", "#FF7BA66D", "#FFCFD08C", "#FFE5A077", "#FFDB685B" },
                new string[] { "#FFFFF9AA", "#FFF5CF40", "#FF707849", "#FF2EA59C", "#FF1A253E" },
                new string[] { "#FF333C46", "#FF5D6F62", "#FFAA9167", "#FFEBA84D", "#FFF2A70F" },
                new string[] { "#FF37AC89", "#FF245475", "#FF676F46", "#FFAC712F", "#FFFED24F" },
                new string[] { "#FF2E3B16", "#FF5D2D30", "#FF43463E", "#FF344321", "#FF5F9320" },
                new string[] { "#FF6E2221", "#FF4D2F18", "#FF18334C", "#FF297C95", "#FF5FAAE0" },
                new string[] { "#FF464741", "#FF6D7661", "#FFAF995A", "#FFEBE2A1", "#FFBBB387" },
                new string[] { "#FFB5B769", "#FF81514D", "#FF313834", "#FF883F45", "#FFE7DA9E" },
                new string[] { "#FF20323C", "#FF5A8B88", "#FF8AB982", "#FFD1CD9A", "#FFDF986D" },
                new string[] { "#FFFC7409", "#FFE79D52", "#FFD4DAA0", "#FFCED8A9", "#FF5AB896" },
                new string[] { "#FFAE302F", "#FFFDD472", "#FFEEE088", "#FF68C0A3", "#FF224C49" },
                new string[] { "#FF3F2B2D", "#FF404032", "#FF4F6D44", "#FF7AB55E", "#FFD1F6B8" },
                new string[] { "#FFA76743", "#FFEFBF3C", "#FFB4B28E", "#FF808354", "#FF979F78" },
                new string[] { "#FFF17151", "#FFF54A4E", "#FF3C4157", "#FF3C6E82", "#FF6E7E9F" },
                new string[] { "#FF462D44", "#FF08598B", "#FF07C110", "#FF80CA25", "#FFFCE84A" },
                new string[] { "#FF369053", "#FF9BC399", "#FFDED0B3", "#FF9769B8", "#FFDC6266" },
                new string[] { "#FFF6E5CF", "#FFFEFEF9", "#FF327A86", "#FF343C3E", "#FF221C1E" },
                new string[] { "#FFD54654", "#FFE74F1B", "#FFDCBD4F", "#FF2DB4A8", "#FF29222C" },
                new string[] { "#FF46404B", "#FFACA4AB", "#FF499097", "#FF7DB294", "#FFFFFAEC" },
                new string[] { "#FF3D2620", "#FF4F464C", "#FFA78C5A", "#FFC1D074", "#FFD2BE30" },
                new string[] { "#FF25525E", "#FF3BCEAC", "#FFCBD2AE", "#FFF15740", "#FFF0191D" },
                new string[] { "#FF18364F", "#FF547472", "#FF3B9FA0", "#FFD94D49", "#FFFFFEFC" },
                new string[] { "#FF095710", "#FF0B760E", "#FF5FAB21", "#FFF37D1B", "#FF90102A" },
                new string[] { "#FF554335", "#FFA6A476", "#FFA58877", "#FFBEAB9F", "#FF5B3A3A" },
                new string[] { "#FF6A2170", "#FF789710", "#FFD7B329", "#FFCD7E14", "#FF921424" },
                new string[] { "#FF2F2437", "#FF32566F", "#FFCCDDC6", "#FFDEC64E", "#FFF46024" },
                new string[] { "#FFF5E9AB", "#FFA07B52", "#FF483847", "#FF202334", "#FF17151C" },
                new string[] { "#FF135883", "#FF9B7FED", "#FF0FD8E7", "#FF95DEAC", "#FFFEB513" },
                new string[] { "#FFE9EA86", "#FF9E714F", "#FF872838", "#FF592B48", "#FF3F1533" },
                new string[] { "#FF0F2B58", "#FF2188BB", "#FF96B4C1", "#FFDFBCA2", "#FFFAF5E6" },
                new string[] { "#FFD8D8B1", "#FF8FA786", "#FF4B7B77", "#FF36485C", "#FF283F4B" },
                new string[] { "#FF3C1217", "#FF501617", "#FF661A17", "#FFD84211", "#FF95B120" },
                new string[] { "#FFE25E60", "#FFCF5FA9", "#FFA9D4BD", "#FF5C96A2", "#FFB4BC80" },
                new string[] { "#FFF08947", "#FFE9A62C", "#FFD8C36F", "#FF9BCC69", "#FF67A893" },
                new string[] { "#FFD14138", "#FFCDA922", "#FF9BB025", "#FF178371", "#FF1B7275" },
                new string[] { "#FF782FF1", "#FF69389D", "#FF556B5F", "#FF3B3430", "#FFE32C2E" },
                new string[] { "#FF81E2B7", "#FF85B590", "#FFA8995F", "#FFB08333", "#FFB43241" },
                new string[] { "#FFBA3510", "#FFD1A236", "#FFFAD087", "#FF929543", "#FF214C3D" },
                new string[] { "#FFA47A4A", "#FFBD999A", "#FFF9D4FA", "#FF87C9BE", "#FF83B2A7" },
                new string[] { "#FF367277", "#FF7FA39D", "#FFDADEC2", "#FFEDBB8F", "#FFD85A5B" },
                new string[] { "#FF81753F", "#FF97AE5D", "#FF939B47", "#FFC1C877", "#FF868A50" },
                new string[] { "#FF354254", "#FF219FA6", "#FFFEFEF9", "#FF8A8D7C", "#FF662F45" },
                new string[] { "#FF794430", "#FF5B4B3B", "#FF2A2527", "#FF565551", "#FF9DA385" },
                new string[] { "#FF281C1F", "#FF3E3426", "#FF5F5D4D", "#FFA3B164", "#FFDCEAB5" },
                new string[] { "#FF6C2443", "#FF915868", "#FF99AF8F", "#FFD7DBA3", "#FFCFDF87" },
                new string[] { "#FF3E5D60", "#FFDAB071", "#FFF4AA4D", "#FFD97F30", "#FFC9293E" },
                new string[] { "#FF4A8A83", "#FF5DC7AF", "#FFF3C78C", "#FFFD8F8D", "#FFD83F5F" },
                new string[] { "#FFB42F51", "#FF4A4E43", "#FF516B6F", "#FF8E8372", "#FFF5EEEE" },
                new string[] { "#FF312F4E", "#FF6F745E", "#FFB1C6AD", "#FFF3D9C3", "#FFE94917" },
                new string[] { "#FF79C89B", "#FF1B5083", "#FF1F6D73", "#FFEC9732", "#FFED422F" },
                new string[] { "#FF3C3F3F", "#FF7D4641", "#FF799181", "#FFB1C8AD", "#FFD9DDCC" },
                new string[] { "#FFD1D7A0", "#FFF9D273", "#FFBBA28F", "#FF336673", "#FF282929" },
                new string[] { "#FF3E4D5D", "#FF79807E", "#FFAB9D8E", "#FFC6D1C9", "#FF8AADC4" },
                new string[] { "#FFE54E47", "#FFFEC411", "#FFEBDC45", "#FF41AA8E", "#FF4D6269" },
                new string[] { "#FF403934", "#FF95966A", "#FFEBD2A7", "#FFD67C53", "#FF593B38" },
                new string[] { "#FF966F41", "#FFB5DFD1", "#FF4AE2BC", "#FF4DAC83", "#FF32616F" },
                new string[] { "#FF1F5170", "#FF2D7180", "#FF46A5AD", "#FFD6DFC2", "#FFD3C47A" },
                new string[] { "#FF283131", "#FF647F72", "#FFD9C88E", "#FFD8AD6F", "#FF695A42" },
                new string[] { "#FF1C565D", "#FF1BAB64", "#FF36D58B", "#FFF5E67A", "#FFFB2837" },
                new string[] { "#FF75A79D", "#FF75CB9A", "#FFE9CEA9", "#FFE8865F", "#FFD63F3C" },
                new string[] { "#FF324537", "#FFA5C2BA", "#FFE5E8DA", "#FF97AD8E", "#FF51635D" },
                new string[] { "#FF84AD91", "#FFCFAE87", "#FFEACFA4", "#FFD69AC8", "#FF604AAE" },
                new string[] { "#FFB8203C", "#FFBF4642", "#FFA39F6C", "#FFE6DD7D", "#FF8E7F37" },
                new string[] { "#FF293948", "#FF247398", "#FFFFFEFD", "#FFC596A4", "#FF67856F" },
                new string[] { "#FF9ACA31", "#FF3E4C52", "#FF6D6362", "#FFF7E5C2", "#FF0EC2E1" },
                new string[] { "#FF745148", "#FFA56B5F", "#FFFBF2D3", "#FFADB987", "#FF4F452F" },
                new string[] { "#FF24272A", "#FF1E2327", "#FF57727B", "#FFB7D4CB", "#FFD9DBCD" },
                new string[] { "#FF4E4D4E", "#FFB5B5AB", "#FFF5F6EC", "#FFE7B374", "#FF9A5E57" },
                new string[] { "#FF182826", "#FFE89B20", "#FFD83423", "#FFA71509", "#FF58060A" },
                new string[] { "#FF483C26", "#FFA7795B", "#FFD1AE5B", "#FFDAA053", "#FFCB4A47" },
                new string[] { "#FF524936", "#FF65887D", "#FFF3C983", "#FFE2B672", "#FF8D684F" },
                new string[] { "#FFB42A31", "#FF7B2056", "#FFE4BB34", "#FF7BD09D", "#FF276F77" },
                new string[] { "#FF55DCD3", "#FF0DD0CE", "#FF109C9C", "#FF116365", "#FF3B5625" },
                new string[] { "#FFBA3B39", "#FF365169", "#FFBF7A4B", "#FFF5CA3D", "#FFF0B453" },
                new string[] { "#FF25A391", "#FF9BE0E2", "#FFE3DBAD", "#FFD2873E", "#FFFA7819" },
                new string[] { "#FF343C3A", "#FF69AEAF", "#FFEDECE3", "#FFF7F6EF", "#FFDA4A4A" },
                new string[] { "#FF3D8F78", "#FFB4B672", "#FFFAD582", "#FFE4B02E", "#FFC77222" },
                new string[] { "#FF2D373D", "#FF697894", "#FF84AECD", "#FFC1D1E5", "#FFE6EEF1" },
                new string[] { "#FF144157", "#FF1AA7AC", "#FFB0D75A", "#FFF7CE60", "#FFE4402B" },
                new string[] { "#FFFDB805", "#FFC99517", "#FF729173", "#FF497563", "#FF1C2828" },
                new string[] { "#FFFEECA0", "#FFD9D177", "#FF9E9263", "#FF4E5747", "#FF252D37" },
                new string[] { "#FFDDF0F9", "#FF6C777F", "#FF8A6A57", "#FFB4622D", "#FF994812" },
                new string[] { "#FF480F3A", "#FF6D3158", "#FFC0576E", "#FFE19F87", "#FFFCC083" },
                new string[] { "#FFC12D51", "#FF102940", "#FF3C8C8C", "#FFC8DE8F", "#FFF4D5B8" },
                new string[] { "#FF344D44", "#FF60AB69", "#FFD9D66D", "#FFA18A30", "#FFC15024" },
                new string[] { "#FF3C2914", "#FF1F2E2B", "#FF1F5659", "#FF2BAA55", "#FFC3BE8C" },
                new string[] { "#FF353437", "#FFFBF5F4", "#FFFB1F70", "#FF4C4A42", "#FF2D4234" },
                new string[] { "#FF3D2234", "#FFBF5945", "#FFEE691C", "#FFF6AE07", "#FF0B9915" },
                new string[] { "#FF714741", "#FF949F7E", "#FFDEDBC1", "#FFB7BC92", "#FF769A83" },
                new string[] { "#FFD2E2B1", "#FF848B7C", "#FF3B4342", "#FF313132", "#FFDF594E" },
                new string[] { "#FF17252C", "#FF1C262B", "#FF567C5C", "#FFADCBA4", "#FFD5EFEF" },
                new string[] { "#FF4EB256", "#FFAD6831", "#FFDC7220", "#FF8E4217", "#FF851D19" },
                new string[] { "#FFBCB785", "#FFF7EAC1", "#FFA5966E", "#FF4D443A", "#FF647F8B" },
                new string[] { "#FF5A2B23", "#FF738B73", "#FFA2AD9A", "#FFECDFBA", "#FF7E7E69" },
                new string[] { "#FF0D2658", "#FF237675", "#FFC7CAA2", "#FF545853", "#FFA62F33" },
                new string[] { "#FFDF3F08", "#FFD5680A", "#FFE1881C", "#FF415251", "#FF523C43" },
                new string[] { "#FF70CFE6", "#FF9CBAB3", "#FFE37340", "#FFF4B519", "#FFF6D010" },
                new string[] { "#FFE18337", "#FFEBE09E", "#FFB98F59", "#FF586157", "#FF342835" },
                new string[] { "#FF587C72", "#FF99ECDC", "#FF8FCFD8", "#FFE2FBF8", "#FFFDDFA7" },
                new string[] { "#FF73A383", "#FF48B3A0", "#FF48968D", "#FF3B7F6A", "#FF0D1222" },
                new string[] { "#FF1C1A25", "#FF4B4A4D", "#FF749395", "#FFA2B4B3", "#FFF4F7F7" },
                new string[] { "#FFDB8F66", "#FFE1DAA8", "#FFD6DADB", "#FF1A3777", "#FF738C9E" },
                new string[] { "#FF314657", "#FF91A99F", "#FFF4F8F8", "#FFD8BCA0", "#FF7D4F32" },
                new string[] { "#FF174A42", "#FF30BB8F", "#FFFBF9F1", "#FFEF5421", "#FFC43536" },
                new string[] { "#FFC24367", "#FFDAC19C", "#FFECE7C9", "#FFA8E1BB", "#FF52A996" },
                new string[] { "#FF251322", "#FFFC0E15", "#FFFFFEFD", "#FF123547", "#FF104046" },
                new string[] { "#FF0594C9", "#FF2A5C6A", "#FF28292D", "#FF661519", "#FFD26A25" },
                new string[] { "#FFFCD597", "#FFD3BE8E", "#FF68888F", "#FF1C3236", "#FF59E5E4" },
                new string[] { "#FF61A54C", "#FFC3E914", "#FFDFED7D", "#FFE06537", "#FF3B1B21" },
                new string[] { "#FFD3E1FB", "#FFF6ECA9", "#FFDCBD38", "#FF5F9972", "#FF553E20" },
                new string[] { "#FF325682", "#FF5D80A8", "#FF7D9ABC", "#FFFBF9F3", "#FFAB5148" },
                new string[] { "#FF746E23", "#FF0E51B0", "#FF168FC6", "#FF18B8DB", "#FF1ACED9" },
                new string[] { "#FF073552", "#FF154F5B", "#FFDA914E", "#FFEB9111", "#FFF79021" },
                new string[] { "#FFFEF9E3", "#FF7CA476", "#FF5E6362", "#FF6A4C71", "#FF36414A" },
                new string[] { "#FF252953", "#FF3FB6B6", "#FFEFF0BC", "#FFF88377", "#FFBF474A" },
                new string[] { "#FFD03E37", "#FFDBD633", "#FF25A49E", "#FF046263", "#FF0B445A" },
                new string[] { "#FF4F4C1A", "#FF0E3D72", "#FF0C79BD", "#FF3594CF", "#FFE1DFD2" },
                new string[] { "#FF383A35", "#FF673D3A", "#FFAAAE9F", "#FFC7CBB3", "#FF9A9C9E" },
                new string[] { "#FFA1B6B1", "#FFE0CFBE", "#FFFFFDF2", "#FF61CCD2", "#FF2E4F5A" },
                new string[] { "#FF2B4D36", "#FF96812A", "#FFF3902D", "#FFDD2316", "#FF8B0A1E" },
                new string[] { "#FFA92929", "#FFB27475", "#FFA7363A", "#FFBB533C", "#FFD78B59" },
                new string[] { "#FF20202A", "#FF313037", "#FF935E3B", "#FFE3C894", "#FFF6ECBA" },
                new string[] { "#FFF9F5F1", "#FFD1A198", "#FF9A7675", "#FF5C5059", "#FF372B2E" },
                new string[] { "#FFFD0C1D", "#FF1ACCAF", "#FFA5E2A0", "#FF75A69A", "#FF374C5B" },
                new string[] { "#FF5A5363", "#FF437FA2", "#FF618FD0", "#FF60ABBB", "#FFB08E82" },
                new string[] { "#FFC6EBF0", "#FF6DC0B4", "#FFA6883D", "#FFBC6859", "#FFC42D2F" },
                new string[] { "#FF112722", "#FF113257", "#FF0786FB", "#FFD2E3DB", "#FFFAAE08" },
                new string[] { "#FF16F1EF", "#FF0FC3C4", "#FF148A77", "#FF837A2F", "#FFD52133" },
                new string[] { "#FF312F3D", "#FF44544E", "#FFB65820", "#FFDC7A26", "#FFF2E6BC" },
                new string[] { "#FF7F1B1F", "#FFE26921", "#FFC8BBA5", "#FF434A53", "#FF272234" },
                new string[] { "#FF889F73", "#FFC0CAB2", "#FFB4B7A2", "#FFBE4E2C", "#FF305037" },
                new string[] { "#FF936A57", "#FFDED799", "#FF979772", "#FF766855", "#FF353424" },
                new string[] { "#FF184D51", "#FF425051", "#FFC0C067", "#FFE2BC41", "#FFEFF211" },
                new string[] { "#FF3F765A", "#FF829737", "#FFCBA739", "#FFD56649", "#FFC64348" },
                new string[] { "#FFDF6739", "#FFE7C472", "#FF7E8768", "#FF314447", "#FF304743" },
                new string[] { "#FF432C50", "#FFA02657", "#FFF63D4B", "#FFF39687", "#FFCABFAF" },
                new string[] { "#FF383E38", "#FF818763", "#FFE0BE2E", "#FFF0D842", "#FF5B4B39" },
                new string[] { "#FFF3E3AB", "#FFBC997A", "#FFA4836E", "#FF524651", "#FF30304B" },
                new string[] { "#FF312B30", "#FF383335", "#FF3CBC85", "#FF9CCAB8", "#FFDDC05E" },
                new string[] { "#FF3C2547", "#FF65364D", "#FF906772", "#FFE4C392", "#FFE8DDC9" },
                new string[] { "#FFFAE679", "#FFFCDB0A", "#FF805A58", "#FF9C0E3C", "#FF0B213A" },
                new string[] { "#FF50676E", "#FFA0B8A8", "#FFF2E59D", "#FFF46A67", "#FFF9F6F3" },
                new string[] { "#FF401128", "#FF7E0F28", "#FFBD4843", "#FFDA906F", "#FFF8E8BA" },
                new string[] { "#FF6B2D2C", "#FFE2803E", "#FFD6A75A", "#FFBCA55C", "#FF47622B" },
                new string[] { "#FF1CB5A6", "#FFD4EAE6", "#FFF0C19D", "#FFEA9145", "#FFEB312A" },
                new string[] { "#FFAD936E", "#FFF9F288", "#FFE0C165", "#FFA6AF6D", "#FFBE9EC7" },
                new string[] { "#FFC63B48", "#FFF9F4EC", "#FFD4C9C3", "#FF9FD3CE", "#FF79A6AD" },
                new string[] { "#FF3FAAA3", "#FF79CBBE", "#FFE7E6D1", "#FFF7EEE0", "#FFC7716B" },
                new string[] { "#FF3A172E", "#FF6F2545", "#FFAD5D61", "#FFE19A8F", "#FFCE984D" },
                new string[] { "#FFD32536", "#FFE6CE89", "#FFB4A64B", "#FF3E5513", "#FF0F2013" },
                new string[] { "#FF053148", "#FF0A8889", "#FFE8D592", "#FFF6CA44", "#FFEC543B" },
                new string[] { "#FFFA1515", "#FFA41415", "#FFDAAE58", "#FFC0D2D0", "#FF3FC6A6" },
                new string[] { "#FFF8D6A2", "#FFAABD92", "#FF4D5E59", "#FF37474F", "#FF2F4B4A" },
                new string[] { "#FFC52865", "#FF411F32", "#FF6C8A5A", "#FF61C08A", "#FFD2EBC3" },
                new string[] { "#FF3C2C43", "#FF858163", "#FFC0B77F", "#FFDED98F", "#FFE7B795" },
                new string[] { "#FFFC5325", "#FFE8E7C6", "#FF56A89B", "#FF202827", "#FF102C25" },
                new string[] { "#FF16616F", "#FFB5C4AE", "#FFDBB589", "#FFF9942F", "#FFF54E15" },
                new string[] { "#FF293F42", "#FF2E363E", "#FF5D9872", "#FFB0D47A", "#FFCBE019" },
                new string[] { "#FFA6152E", "#FFD4724E", "#FFE6D9D0", "#FFB1BEC6", "#FF2F4355" },
                new string[] { "#FF4F864A", "#FFC9C449", "#FFE16C3F", "#FFE55224", "#FF752F22" },
                new string[] { "#FF2E483C", "#FF3E6240", "#FF75A764", "#FFBCBE45", "#FFEBE560" },
                new string[] { "#FF2C5A58", "#FF55A896", "#FFFEFDFB", "#FFDBC35A", "#FFFC504E" },
                new string[] { "#FF5A3C3F", "#FFC99399", "#FFB07177", "#FFD45A6A", "#FFBF9D8A" },
                new string[] { "#FFA2A169", "#FFC1CC3B", "#FFD19B2D", "#FF534530", "#FF342718" },
                new string[] { "#FF2F1C1D", "#FF8F5F60", "#FFB5888D", "#FF523F42", "#FF17242A" },
                new string[] { "#FFCB5255", "#FFE56B3A", "#FFEFDEA8", "#FF4CB6AE", "#FF1D5B92" },
                new string[] { "#FFBFC54D", "#FFAA8C11", "#FF8C521A", "#FF4F3B23", "#FF31191A" },
                new string[] { "#FF283011", "#FF727E44", "#FFBD9947", "#FFCFBB60", "#FF806B3E" },
                new string[] { "#FF55709E", "#FFC3C3C4", "#FFFCF9F7", "#FF494147", "#FFEA8011" },
                new string[] { "#FF39EC81", "#FF18966C", "#FF1B6D3B", "#FF2F472B", "#FFDA7823" },
                new string[] { "#FF1E3435", "#FF376068", "#FF76B0C7", "#FFFDFEFE", "#FFD31E0D" },
                new string[] { "#FF191A1D", "#FF3B7386", "#FF6BB3C7", "#FFE9DABF", "#FFEE7C6F" },
                new string[] { "#FF4C2942", "#FF884253", "#FFDE873E", "#FFDABE78", "#FFB2BF44" },
                new string[] { "#FFE12C49", "#FFFAF7E2", "#FF99D4D4", "#FF4F7C86", "#FF24383D" },
                new string[] { "#FF1B101B", "#FF191D29", "#FF213B42", "#FFBEBD98", "#FFE3D36F" },
                new string[] { "#FF142B2F", "#FF2A7A71", "#FF9DCD51", "#FFF9ED7D", "#FFF6550C" },
                new string[] { "#FF2D3433", "#FF6E925D", "#FFA7B061", "#FF63A74E", "#FFB9CC1F" },
                new string[] { "#FF162430", "#FF36777C", "#FFBED783", "#FFE8DAB2", "#FFDA7354" },
                new string[] { "#FF513534", "#FF918E93", "#FFB4B8A2", "#FFDBC3BE", "#FFCB4F49" },
                new string[] { "#FFE5BE9E", "#FFD5CFAE", "#FFB3B9A7", "#FF658780", "#FF423D2E" },
                new string[] { "#FF555E5E", "#FFC2BD7E", "#FFFBC246", "#FFE1AD63", "#FF7C8364" },
                new string[] { "#FF7F7E50", "#FF9D8E6A", "#FFD9D3AC", "#FF97BBB2", "#FFC6473C" },
                new string[] { "#FF462730", "#FF8D7285", "#FFD0B5C1", "#FFDFDEDC", "#FF58545C" },
                new string[] { "#FFA2463E", "#FFC09E55", "#FFD8CF71", "#FFCEC19C", "#FF8D7C6E" },
                new string[] { "#FF3F6263", "#FF4FCEB5", "#FFF6E445", "#FFFAA72A", "#FFCE324F" },
                new string[] { "#FF445A65", "#FF8AAAB5", "#FFBBE3E2", "#FFF3C177", "#FF424344" },
                new string[] { "#FF352A26", "#FF574D44", "#FF468967", "#FFA2AB86", "#FFF4F0C9" },
                new string[] { "#FFDE1912", "#FFFBF9F5", "#FF8DCED2", "#FF4F9999", "#FF262F29" },
                new string[] { "#FF747480", "#FF282731", "#FF8B2D1A", "#FFD9381A", "#FFE6BC6A" },
                new string[] { "#FFD9E3DD", "#FFCDBBAA", "#FF857A65", "#FF264E64", "#FF0F2E35" },
                new string[] { "#FF33322C", "#FF473E4D", "#FF857C64", "#FF9AA99E", "#FFD6E0CD" },
                new string[] { "#FFCF5A63", "#FFF4F3F0", "#FFC9B2AF", "#FFA08181", "#FF342C2A" },
                new string[] { "#FF0F6A7E", "#FF5ABAA7", "#FFEBD593", "#FFCDA779", "#FF594139" },
                new string[] { "#FFA2A729", "#FFC6C133", "#FF9C8933", "#FF61641C", "#FF34211C" },
                new string[] { "#FF4E3530", "#FF84B651", "#FFADCA5C", "#FF71B95D", "#FF6F7C56" },
                new string[] { "#FFD0C28F", "#FFC5AA79", "#FFA27E55", "#FFB33435", "#FFA18011" },
                new string[] { "#FF555347", "#FF4C6262", "#FF979D8E", "#FFD3E6D3", "#FFD2AF3D" },
                new string[] { "#FF629090", "#FF5A7B74", "#FF4F836F", "#FF2D453A", "#FF3D3A22" },
                new string[] { "#FF17292B", "#FFF4DCAD", "#FFE7B86B", "#FFDB2A18", "#FF5B1209" },
                new string[] { "#FFBDD19E", "#FFA5A97C", "#FF999875", "#FFDE5551", "#FF38342C" },
                new string[] { "#FFE4AA2F", "#FFD0C582", "#FF3BA2AE", "#FF2A7C81", "#FF21354D" },
                new string[] { "#FF203E46", "#FF364F66", "#FF53A281", "#FF63A94D", "#FFC6DB8E" },
                new string[] { "#FF870C0A", "#FFAC6559", "#FFDFB577", "#FFEAC8A9", "#FF874139" },
                new string[] { "#FF253A2D", "#FF317B5E", "#FF9FB965", "#FFF2E274", "#FFF6B241" },
                new string[] { "#FF3C494E", "#FF61807A", "#FFB5AF87", "#FFECE4C0", "#FFE19279" },
                new string[] { "#FFC9365B", "#FFFFD248", "#FFACBD5D", "#FF22958C", "#FF16484C" },
                new string[] { "#FFCD514A", "#FFCC9944", "#FFDAD950", "#FFC7CD79", "#FF5F989C" },
                new string[] { "#FF0D2B73", "#FF234BA6", "#FF467BBC", "#FF60B1B9", "#FF44A42E" },
                new string[] { "#FF211618", "#FF8A0F0E", "#FFD27D39", "#FFC4B28E", "#FFC8AB96" },
                new string[] { "#FF112E44", "#FF1AA867", "#FFF7F6EC", "#FF87A61F", "#FF822A19" },
                new string[] { "#FFAAD9A3", "#FF479086", "#FF155853", "#FF223B51", "#FF131D39" },
                new string[] { "#FF766350", "#FF8B8B81", "#FFDFCEA6", "#FFBAAD8C", "#FF98A07B" },
                new string[] { "#FF1B2B0E", "#FF688E16", "#FFAAA11D", "#FFF0BE1B", "#FF5A5047" },
                new string[] { "#FF182332", "#FF32526C", "#FFB3C8D3", "#FFFCFDFC", "#FFD49B1D" },
                new string[] { "#FF991325", "#FFA21023", "#FFB93A25", "#FFA57958", "#FF769946" },
                new string[] { "#FF50313B", "#FF893F55", "#FF8D6643", "#FFB1BC9D", "#FFE8D6C6" },
                new string[] { "#FF5C5F6C", "#FFA68C69", "#FFD1581C", "#FF742507", "#FF42170D" },
                new string[] { "#FF463839", "#FF706445", "#FF68854F", "#FF7A8C48", "#FF73953A" },
                new string[] { "#FF7E8C45", "#FFF8ECD3", "#FF3F2D1D", "#FF753B24", "#FF6E3B45" },
                new string[] { "#FFF4E7C1", "#FFD0CC7C", "#FF968E5C", "#FF4E553B", "#FF231F1C" },
                new string[] { "#FF41403C", "#FF719E91", "#FFC3E6E1", "#FFCACBBD", "#FFBE5842" },
                new string[] { "#FF08202D", "#FF10212E", "#FF576669", "#FFC8BBA7", "#FFE6D8CB" },
                new string[] { "#FF236172", "#FF214D66", "#FF26221B", "#FFF3620C", "#FFF53F0F" },
                new string[] { "#FF101E3E", "#FF2E8A8D", "#FFF6F3E6", "#FFB7B2A4", "#FFB44F43" },
                new string[] { "#FF242F36", "#FF34586E", "#FF598F9A", "#FFE7ECE4", "#FFD18E7F" },
                new string[] { "#FF443944", "#FF58695A", "#FF92B09F", "#FFD0F81C", "#FFDEF3A5" },
                new string[] { "#FFFFFEFD", "#FFEA9122", "#FF396B78", "#FFCCD6DD", "#FF242B28" },
                new string[] { "#FF05313A", "#FF0B2F4B", "#FF0DA6AD", "#FFF1ECD4", "#FFE87985" },
                new string[] { "#FF483D48", "#FF74B999", "#FFF9F3BE", "#FFF7D376", "#FF79DF9A" },
                new string[] { "#FFCCA9A0", "#FFFDE8BE", "#FFF2F5C4", "#FFBAE1D6", "#FFEBEDFA" },
                new string[] { "#FF674C41", "#FFAD7E6C", "#FFCD977E", "#FFD3BB9C", "#FF73846C" },
                new string[] { "#FF1C0E25", "#FFC90C0E", "#FFDFD5C3", "#FFF5DEAF", "#FF423330" },
                new string[] { "#FFC2BC8D", "#FFA4AB8F", "#FF606257", "#FF1D2428", "#FF6A6A61" },
                new string[] { "#FFE34B37", "#FFD1B472", "#FFA9D8AA", "#FF2F9A3A", "#FF26332D" },
                new string[] { "#FF4C5237", "#FFAC794E", "#FFA27638", "#FF86531B", "#FF7A2E0D" },
                new string[] { "#FF8E765C", "#FFF7F5F0", "#FFC8AEB3", "#FFA36D7D", "#FF542D3C" },
                new string[] { "#FFFA293E", "#FF3C4A3B", "#FF6EB363", "#FFE2E9D1", "#FFA09E84" },
                new string[] { "#FFC72D38", "#FFFB9D14", "#FFF6CF31", "#FF85D08F", "#FF5EC1C2" },
                new string[] { "#FF0D1F29", "#FF202E3E", "#FF15545C", "#FF60A2A5", "#FFFAE4DE" },
                new string[] { "#FF3A3453", "#FF546070", "#FFC7EE64", "#FFECE9B3", "#FFC5A445" },
                new string[] { "#FFF04F20", "#FF853060", "#FF0B73DA", "#FF1F9D8B", "#FF80D4EE" },
                new string[] { "#FF959971", "#FFF28D8D", "#FFF31B9A", "#FF85325B", "#FFCF1579" },
                new string[] { "#FF13293B", "#FF1B4B5D", "#FF569593", "#FFDFD3A7", "#FFE8AA91" },
                new string[] { "#FF3D302B", "#FF3F3B2D", "#FF8F9A5A", "#FFA0CFA0", "#FFE1E7A7" },
                new string[] { "#FFF7DCB7", "#FFD4BC92", "#FF959676", "#FF6A5E68", "#FF4D2B3F" },
                new string[] { "#FF443A3B", "#FF89AC82", "#FFE4CFAC", "#FFC8B390", "#FF866263" },
                new string[] { "#FF333435", "#FF4D5451", "#FF70766B", "#FF9EB49E", "#FFF5F2EB" },
                new string[] { "#FFD3BCD8", "#FFB2EFD5", "#FFD0D5BD", "#FFC0818E", "#FF9F8972" },
                new string[] { "#FF213C47", "#FF3B7770", "#FFCCE8B3", "#FFFCE376", "#FFF97661" },
                new string[] { "#FF3BA03D", "#FFC2AB2D", "#FFC35321", "#FFC01217", "#FF96131E" },
                new string[] { "#FF1A8580", "#FF67B09F", "#FFFCD357", "#FFF58B25", "#FFE3342C" },
                new string[] { "#FF37614D", "#FF519646", "#FF89C323", "#FFB3CA4C", "#FFE9F8EA" },
                new string[] { "#FF2D313A", "#FF6BC5BE", "#FFF4E4AA", "#FFBC8527", "#FF8D5F0F" },
                new string[] { "#FF443045", "#FF90A635", "#FFFEF4BA", "#FF56C7CF", "#FF0B3F5A" },
                new string[] { "#FF154466", "#FF0BB7BD", "#FF4ED3C8", "#FF12E4AF", "#FFCCDAA5" },
                new string[] { "#FF0C252A", "#FF207962", "#FFB1DA73", "#FFCEE3A0", "#FFCE7732" },
                new string[] { "#FFD0C1A2", "#FFC9AD60", "#FF9C8B5A", "#FF9BBAA6", "#FF9FE6ED" },
                new string[] { "#FFE9EEA0", "#FFB1CBB0", "#FF868E91", "#FF607477", "#FF4E446D" },
                new string[] { "#FFF8FAF8", "#FF07AA78", "#FF37693C", "#FF6A4318", "#FF4A342B" },
                new string[] { "#FF97745A", "#FFD3BAA8", "#FF8FCDCB", "#FFDDC9A6", "#FFD6B6A2" },
                new string[] { "#FF412B39", "#FF323B41", "#FF6A836C", "#FFDECB8D", "#FFE8B65C" },
                new string[] { "#FF7FAF85", "#FFFDFBF3", "#FFDEE1D1", "#FF6C7D97", "#FF363741" },
                new string[] { "#FFC85029", "#FFE4AB5D", "#FFA79733", "#FF3F4D11", "#FF371C2F" },
                new string[] { "#FF805839", "#FFBD9862", "#FFE5CE9D", "#FF578272", "#FF252C23" },
                new string[] { "#FF434745", "#FF97AE32", "#FFFDF1B6", "#FFB3BFBE", "#FF49394A" },
                new string[] { "#FFBE0F34", "#FFF44C76", "#FFE9C3AA", "#FFFDFDF8", "#FFF38B25" },
                new string[] { "#FFC5746B", "#FFC3BFBC", "#FFE6E1E0", "#FFE5DDDA", "#FFDCB5B9" },
                new string[] { "#FF1C6F95", "#FF2FAAB0", "#FFF5F4EA", "#FFCFA280", "#FFB83737" },
                new string[] { "#FF202344", "#FF555D78", "#FF50758D", "#FF77B9C5", "#FF96C0B8" },
                new string[] { "#FF162933", "#FF34424D", "#FF738B88", "#FFA5BE8F", "#FFF8EBB4" },
                new string[] { "#FFB0C167", "#FFDDB73A", "#FF86845F", "#FFAF5A44", "#FF324132" },
                new string[] { "#FFAD3341", "#FFE0E2D9", "#FFE8E0D7", "#FF2E2424", "#FF300C10" },
                new string[] { "#FF243334", "#FF181A1A", "#FF322E34", "#FF49475B", "#FFEBBB62" },
                new string[] { "#FF1C2420", "#FF9CB96B", "#FFC7A964", "#FF8A663D", "#FF3C1617" },
                new string[] { "#FF31234B", "#FF333057", "#FF08A687", "#FFB3DB61", "#FFE5F2BA" },
                new string[] { "#FF0D4B53", "#FFCABD92", "#FFDC943D", "#FF68661F", "#FF782229" },
                new string[] { "#FF6E6E58", "#FFCFB677", "#FFF5E28F", "#FF989970", "#FF51443A" },
                new string[] { "#FFFB277F", "#FF8A7677", "#FFCCC694", "#FFF7C286", "#FFDEE8CE" },
                new string[] { "#FF2B1828", "#FF7F2A1A", "#FFFA9126", "#FFE1C77A", "#FF3AB29A" },
                new string[] { "#FF543B2F", "#FFB07D37", "#FFFCFCFA", "#FF096773", "#FF193332" },
                new string[] { "#FF896D7F", "#FF562A46", "#FF642D53", "#FFC37D8E", "#FFD1B27E" },
                new string[] { "#FF2D4D5B", "#FF439F74", "#FFCAB73D", "#FFF9B43D", "#FFE54E54" },
                new string[] { "#FF393543", "#FF71787F", "#FFD1B6A8", "#FFD5BC92", "#FFC7583A" },
                new string[] { "#FFF7BF0E", "#FFE6B55E", "#FFAA9058", "#FF565D5C", "#FF2F3A45" },
                new string[] { "#FFB5DBEB", "#FFBADDDA", "#FF60BDEB", "#FF5C9AAD", "#FF4C97CC" },
                new string[] { "#FF4A4040", "#FF70836E", "#FFCAD73A", "#FFE1BF6F", "#FFFAF5F0" },
                new string[] { "#FF718395", "#FF91C8BC", "#FFA48870", "#FFAC7366", "#FFBD5549" },
                new string[] { "#FF579F75", "#FFD7D5CF", "#FFD9C48B", "#FFB76345", "#FF7B443A" },
                new string[] { "#FFBD1721", "#FF566953", "#FF4F8D6E", "#FF70A670", "#FFB7B671" },
                new string[] { "#FFE8DFDA", "#FF80869B", "#FF251836", "#FF411E3A", "#FF977D53" },
                new string[] { "#FF0AAEDA", "#FF28ABC3", "#FFEFCD85", "#FFEB7935", "#FF383638" },
                new string[] { "#FF74675B", "#FFA7A1A0", "#FFCFBAAD", "#FFBAB3B0", "#FFECEDE8" },
                new string[] { "#FFBF2832", "#FFFC434B", "#FFA4A06A", "#FF44DCDE", "#FFFEFEFE" },
                new string[] { "#FF5A5947", "#FFC5B17A", "#FFE2D8AB", "#FFFBF3D8", "#FF574334" },
                new string[] { "#FF1A5261", "#FF52BA83", "#FFEAE578", "#FFFBAC39", "#FFF6572B" },
                new string[] { "#FFE0150F", "#FFF2E1BE", "#FFA29675", "#FF3C4246", "#FF16222B" },
                new string[] { "#FF132940", "#FF254F5B", "#FF6B93A4", "#FFABC3DD", "#FFE3EDEA" },
                new string[] { "#FF874342", "#FFE3A260", "#FFCCD57D", "#FF228B63", "#FF15352B" },
                new string[] { "#FF31CF71", "#FF07576E", "#FF344A5E", "#FFF39C0F", "#FFF3324A" },
                new string[] { "#FF72210A", "#FFF1985B", "#FFA8B0B7", "#FF09456B", "#FF0C1118" },
                new string[] { "#FF243A49", "#FF3A505A", "#FF43717F", "#FFB6C0AB", "#FFFBF7E7" },
                new string[] { "#FF3EBB7C", "#FFB9D967", "#FFFAAC1F", "#FFF96B20", "#FFE02B30" },
                new string[] { "#FFE7B37B", "#FFCD7C3C", "#FF815F47", "#FF4B4842", "#FF4B3429" },
                new string[] { "#FF286167", "#FF14AFA1", "#FFA1D6BD", "#FFFE9F85", "#FFE22436" },
                new string[] { "#FFCBDAF2", "#FFACB8DC", "#FF7A9AAE", "#FF6D562D", "#FF779928" },
                new string[] { "#FFFAF5B0", "#FFCECF98", "#FFBCCFA7", "#FF9BD7A7", "#FFAEAFA5" },
                new string[] { "#FF71726C", "#FF898B86", "#FFE5E7BB", "#FFBAAA60", "#FFC36842" },
                new string[] { "#FFD7436B", "#FF869288", "#FFE98548", "#FFF28234", "#FFE9995C" },
                new string[] { "#FF578531", "#FFEB3961", "#FFFE3606", "#FF621C13", "#FF2C1327" },
                new string[] { "#FF72BD80", "#FFFBE16B", "#FFCDC67B", "#FFBA6F55", "#FFCD333E" },
                new string[] { "#FF13272B", "#FF24232F", "#FFDABE94", "#FFF8DEAE", "#FFE56451" },
                new string[] { "#FFFDF707", "#FFF5B583", "#FFA0C1D4", "#FF08F8FB", "#FF0C3060" },
                new string[] { "#FFFE7216", "#FFD64A1E", "#FF9E3224", "#FF3A2E3E", "#FF0B5357" },
                new string[] { "#FF302B2D", "#FF57575D", "#FF5BA0A6", "#FFD4D9D9", "#FFFEFCFA" },
                new string[] { "#FFDDC828", "#FFACB35E", "#FF4D750F", "#FF292A2B", "#FF242125" },
                new string[] { "#FF236450", "#FF536C44", "#FF9FEA5B", "#FF446A27", "#FF3F4D2D" },
                new string[] { "#FFE8281B", "#FFFD772C", "#FFF0E441", "#FF27B98C", "#FF104F61" },
                new string[] { "#FF096F83", "#FF77A1A6", "#FFFDEDC3", "#FFAF1213", "#FF8E0E0D" },
                new string[] { "#FF77C6DB", "#FF425363", "#FF838B67", "#FF929C79", "#FFDECF4F" },
                new string[] { "#FF7A3A5B", "#FFCA8D2C", "#FFFBBC10", "#FFAE6723", "#FF309465" },
                new string[] { "#FF14363E", "#FF128192", "#FFFDE3A6", "#FFF6BB51", "#FFD74C38" },
                new string[] { "#FF221819", "#FF6F9445", "#FFD0B863", "#FFE2D058", "#FFAC472A" },
                new string[] { "#FFF08C56", "#FFB8C39F", "#FFDBE0D2", "#FF76C1C1", "#FF51466C" },
                new string[] { "#FFBB4132", "#FFB98646", "#FFD0D8B2", "#FF57AFAA", "#FF398196" },
                new string[] { "#FF825F39", "#FFA88841", "#FFF0BB49", "#FFDD6D26", "#FF3C261F" },
                new string[] { "#FF3F3D48", "#FF636B65", "#FFAB9070", "#FFE5AB66", "#FFF4B53A" },
                new string[] { "#FFCF2616", "#FF861826", "#FF48152B", "#FF4D0F3A", "#FF1E1521" },
                new string[] { "#FF255557", "#FF6EA080", "#FFEDBA27", "#FFF2681E", "#FFC53826" },
                new string[] { "#FF3A262F", "#FFA15F47", "#FFB3B67B", "#FFD9E1C3", "#FFAFD9D9" }
            };

        public static xColor EarthColor(Int32 numOfSteps, Int32 step)
        {
            StepCheck(numOfSteps, ref step);
            double n = (double)(EarthColors.Length - 2) / numOfSteps * step;
            int i = (int)Math.Truncate(n);
            xColor clr1 = EarthColors[i];
            xColor clr2 = EarthColors[i + 1];

            double nSteps = (double)numOfSteps / EarthColors.Length - 1;
            double nStep = (double)nSteps / numOfSteps * step;

            return GradientColor(clr1, clr2, nSteps, nStep);
        }

        public static xColor GradientColor(xColor[] clrS, double numOfSteps, double step)
        {
            if (clrS.Length < 2)
                return clrS[0];
            StepCheck(numOfSteps, ref step);
            double n = (double)(clrS.Length - 1) / numOfSteps * step;
            int i = (int)Math.Truncate(n);
            if (clrS.Length < i + 2)
                return clrS[clrS.Length - 1];
            xColor clr1 = clrS[i];
            xColor clr2 = clrS[i + 1];

            double nSteps = (double)numOfSteps / (clrS.Length - 1);
            double nStep = step - nSteps * i;

            //xLog.Debug("GradientColor", step + "/" + numOfSteps + " => " + i + "/" + clrS.Length + ": " + nStep + "/" + nSteps);

            return GradientColor(clr1, clr2, nSteps, nStep);
        }

        public static xColor GradientColor(xColor clr1, xColor clr2, double numOfSteps, double step)
        {
            StepCheck(numOfSteps, ref step);
            double n = numOfSteps / step;
            /* 
            double h = clr1.Hue + (clr2.Hue - clr1.Hue) / n;
            double s = clr1.Saturation + (clr2.Saturation - clr1.Saturation) / n;
            double l = clr1.Luminosity + (clr2.Luminosity - clr1.Luminosity) / n;
            double a = clr1.A + (clr2.A - clr1.A) / n;
            if (l < .1 || l > .9)
                l.ToString();
            return Color.FromHsla(h, s, l, a);
            */
            double r = clr1.R + (clr2.R - clr1.R) / n;
            double g = clr1.G + (clr2.G - clr1.G) / n;
            double b = clr1.B + (clr2.B - clr1.B) / n;
            double a = clr1.A + (clr2.A - clr1.A) / n;

            return xColor.FromRgba((int)(r * 255), (int)(g * 255), (int)(b * 255), (int)(a * 255));
        }

        /// <summary>Blends the specified colors together.</summary>
        /// <param name="color">Color to blend onto the background color.</param>
        /// <param name="backColor">Color to blend the other color onto.</param>
        /// <param name="amount">How much of <paramref name="color"/> to keep,
        /// “on top of” <paramref name="backColor"/>.</param>
        /// <returns>The blended colors.</returns>
        public static xColor Blend(this xColor color, xColor backColor, double amount)
        {
            if (backColor == xColor.Transparent)
                return color;
            double r = (color.R * amount) + backColor.R * (1 - amount);
            double g = (color.G * amount) + backColor.G * (1 - amount);
            double b = (color.B * amount) + backColor.B * (1 - amount);
            return new xColor(r, g, b);
        }
    }

    public class DateGradient
    {
        public List<DynamicGradient> GradientS { get; set; } = new List<DynamicGradient>();

        public xColor GetColor(DynamicDate dDate)
        {
            xColor clr = xColor.Transparent;
            foreach (var g in GradientS)
            {
                if (g.IsActive)
                    clr = DynamicColors.Blend(g.GetColor(dDate), clr, .5);
            }
            return clr;
        }
    }

    public class DynamicGradient
    {
        public DynamicGradient() { } //for xml..

        public DynamicGradient(TimeUnit timeUnit, GradientType gradientType)
        {
            TimeUnit = timeUnit;
            GradientType = gradientType;
        }
        public DynamicGradient(TimeUnit timeUnit, xColor[] customColors)
        {
            TimeUnit = timeUnit;
            GradientType = GradientType.CustomColors;
            CustomColors = customColors;
        }

        public bool IsActive { get; set; } = true;
        public GradientType GradientType { get; set; } = GradientType.Rainbow;
        public TimeUnit TimeUnit { get; set; } = TimeUnit.Year;
        public int StartValue { get; set; }
        public double StartPercentage { get; set; }
        public double StartAngle { get; set; }
        public xColor[] CustomColors { get; set; }

        public xColor GetColor(DynamicDate dDate)
        {
            if (dDate.IsEmpty)
                return xColor.Transparent;

            if (GradientType == GradientType.StaticColor)
                return CustomColors?[0] ?? xColor.Transparent;
            else if (GradientType == GradientType.RandomColor)
                return DynamicColors.RandomColor();

            int iMax = 1;
            int iPos = 0;
            var model = dDate.Model;

            switch (TimeUnit)
            {
                case TimeUnit.Day:
                    return xColor.Transparent;

                case TimeUnit.Week:
                    iMax = model.WeekLength - 1;
                    iPos = dDate.DayOfWeek - model.FirstDayOfWeek;
                    break;

                case TimeUnit.Month:
                    iMax = model.GetDaysOfMonth(dDate.Year, dDate.Month) - 1;
                    iPos = dDate.Day;
                    if (dDate.Month == 0 && model.FirstMonthFirstDayType == FirstMonthFirstDayType.ContinueLastYear)
                        iPos += model.GetYearInfo(dDate.Year).FirstMonthFirstDayNumber;
                    break;

                case TimeUnit.Year:
                    iMax = model.GetDaysOfYear(dDate.Year) - 1;
                    iPos = dDate.DayOfYear;
                    break;
            }

            return GetColor(dDate, iMax, iPos);
        }
        public xColor GetColor(DynamicDate dDate, double numOfSteps, double step)
        {
            if (dDate.IsEmpty)
                return xColor.Transparent;

            if (GradientType == GradientType.StaticColor)
                return CustomColors?[0] ?? xColor.Transparent;
            else if (GradientType == GradientType.RandomColor)
                return DynamicColors.RandomColor();

            if (StartValue != 0)
                step += StartValue;

            if (StartPercentage != 0)
                step += numOfSteps * StartPercentage / 100.0;

            if (StartAngle != 0)
                step += numOfSteps * StartAngle / 360.0;

            switch (this.GradientType)
            {
                case GradientType.Rainbow:
                    return DynamicColors.RainbowColor((int)numOfSteps, (int)step);

                case GradientType.EarthColors:
                    return DynamicColors.EarthColor((int)numOfSteps, (int)step);

                case GradientType.CustomColors:
                    return DynamicColors.GradientColor(CustomColors, numOfSteps, step);
            }

            return xColor.Transparent;
        }
    }

    public enum GradientType
    {
        StaticColor,
        RandomColor,
        Rainbow,
        EarthColors,
        CustomColors,
    }
}