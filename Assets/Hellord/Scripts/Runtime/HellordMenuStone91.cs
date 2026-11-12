using UnityEngine;
using System.Collections.Generic;
namespace Hellord {
 public partial class HellordGame {
  // Shared menu chrome in the existing 700-unit canvas; original Sins stone pixels, never generated art.
  bool _currencyFit91=true; int _summaryCount91;
  void MenuFrame91(float x,float y,float w,float h,bool fill=true,float border=0){
   SinArt74Ready();float b=border>0?border:h<18?1:h<60?2:6;
   float nx=x/K74,ny=y/K74,nw=w/K74,nh=h/K74;
   if(fill)R53(x,y,w,h,new Color32(25,24,21,255));
   float[] sx={126,126+b,166-b,166},sy={46,46+b,86-b,86},dx={nx,nx+b,nx+nw-b,nx+nw},dy={ny,ny+b,ny+nh-b,ny+nh};
   for(int j=0;j<3;j++)for(int k=0;k<3;k++)if(j!=1||k!=1)StoneTile89(new Rect(sx[k],sy[j],sx[k+1]-sx[k],sy[j+1]-sy[j]),new Rect(dx[k],dy[j],dx[k+1]-dx[k],dy[j+1]-dy[j]));
  }
  void CurrencyCounter91(float x,float y,bool sigil,float w=86,float h=29,bool inline=false){
   SinArt74Ready();StonePlaque89(x,y,w,h,3);float iy=y+(h-12)/2;if(!inline){Box74(x+5,iy-2,17,16,new Color32(13,12,11,255));Box74(x+7,iy-3,13,18,new Color32(13,12,11,255));Box74(x+7,iy+14,13,1,new Color32(78,71,58,255));}else Box74(x+5,iy,16,12,new Color32(13,12,11,255));
   var t=T53(sigil?"sigil":"soul");if(t){GUI.color=Color.white;GUI.DrawTexture(new Rect(U74(x+7),U74(iy),U74(12),U74(12)),t);}
   string label=sigil?"SIGILS":"SOULS";int n=Shown48(sigil,sigil?_sigils51:_souls51);string value=n.ToString("N0",System.Globalization.CultureInfo.InvariantCulture);float tx=x+27,vy=inline?y+(h-6)/2:y+18,available=inline?w-32-TW74(label)-5:w-32;
   if(TW74(value)>available)value=Num88(n);_currencyFit91 &= TW74(value)<=available&&TW74(label)<=w-32;
   Tx74(label,tx,inline?vy:y+6,S74_SAND);Tx74(value,inline?tx+TW74(label)+5:tx,vy,Rolling48?Heat48:S74_SAND);
  }
  void Currency70091(float x,float y,bool sigil,float w=125.4167f,float h=42.2917f,bool inline=false)=>CurrencyCounter91(x/K74,y/K74,sigil,w/K74,h/K74,inline);
  void MenuButton91(float x,float y,float w,float h,string label,bool enabled=true){
   bool primary=label=="OPEN"||label.StartsWith("UNLOCK")||label=="SIGN"||label=="TAKE NODE"||label=="REMOVE"||label=="CHOOSE PATH"||label=="SEAL AND DESCEND"||label=="YES";
   if(primary&&enabled){SinArt74Ready();Patch88(_s74bg,new Rect(325,204,69,29),new Rect(x/K74,y/K74,w/K74,h/K74),3);}else MenuFrame91(x,y,w,h);
   TxC97(label,x+w/2,y+(h-7.65f)/2,enabled?S74_SAND:S74_DIM);
  }
  void PactHeader91(string title){
   _currencyFit91=true;Currency70091(80,22,false);Currency70091(495,22,true);
   float tw=TW97(title,2)+24;MenuFrame91(350-tw/2,22,tw,26);TxC97(title,350,27.5f,S74_SAND,2);
   MenuFrame91(210,49,280,14);TxC97("PATH "+(_pactPath51>=0?PactData.PathShort[_pactPath51]:"NONE")+" / PICK "+SelCount51()+"/"+MaxSel51(),350,52,S74_SAND);
  }
  void DrawYourPact91(){
   var bg=T53("altar_bg",true);if(bg)GUI.DrawTexture(new Rect(0,0,700,392),bg);R53(0,0,700,392,new Color(0,0,0,.75f));MenuFrame91(64,12,572,368);PactHeader91("YOUR PACT");
   int p=_pactPath51;var selected=new List<int>();if(p>=0)for(int i=0;i<PactData.Nodes[p].Length;i++)if(Taken97(p,i))selected.Add(i);_summaryCount91=selected.Count;
   int mx=MaxSel51();TxC97(mx+" SLOTS / ONE PATH / "+MaxKeys51()+" KEYSTONE MAX",350,76,S74_SAND);
   for(int k=0;k<mx;k++){
    float x=124+(k%4)*120,y=94+(k/4)*70;MenuFrame91(x,y,52,52);
    if(k<selected.Count){int n=selected[k];I97(T97("icon_"+(p==1?4:p)+"_"+n),x+7,y+7,38,38);TxC97(PactData.Nodes[p][n].id,x+26,y+54,S74_SAND);}else TxC97("EMPTY",x+26,y+54,S74_DIM);
   }
   bool crest=p>=0&&selected.Count>=PT.pactCrestAt51;TxC97(crest?PactData.PathShort[p]+" CREST ACTIVE":"CREST AT "+PT.pactCrestAt51+"+ FROM ONE PATH",350,235,crest?S74_GOLD:S74_SAND);
   MenuFrame91(104,250,492,70);for(int k=0;k<selected.Count;k++)Tx97(PactData.Nodes[p][selected[k]].id+" "+PactData.Nodes[p][selected[k]].name,116+(k%2)*238,260+(k/2)*12,S74_SAND);
   MenuButton91(104,338,170,24,"SEAL AND DESCEND");if(Click53(104,338,170,24)){Descend51();return;}
   MenuButton91(486,338,110,24,"BACK");if(Click53(486,338,110,24))_altarScr53=0;
  }
 }
}
