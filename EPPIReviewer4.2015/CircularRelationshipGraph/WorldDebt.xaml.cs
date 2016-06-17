using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml.Linq;
using CircularRelationshipGraph.Data;

namespace CircularRelationshipGraph
{
  public partial class WorldDebt : UserControl
  {
    public WorldDebt()
    {
      InitializeComponent();

      var doc = XDocument.Parse(_xml);
      var nodes = doc.Descendants("country")
                 .Select(el => new Node()
                 {
                   Name = el.Attribute("name").Value,
                   Count = double.Parse(el.Attribute("debt").Value),
                   Tag = el.Attribute("text").Value,
                   Relationships = el.Descendants("owes")
                                  .Select(rel => new NodeRelationship()
                                  {
                                    To = rel.Attribute("name").Value,
                                    Strength = double.Parse(rel.Attribute("amount").Value)
                                  }).Cast<INodeRelationship>().ToList()
                 }).Cast<INode>().ToList();

      graph.Data = new NodeList(nodes);
    }



    private static string _xml = @"
<debt>
  <country name='France' debt='4200' text='Europes second biggest economy owes the UK, the US and Germany the most money. However, like in Germanys case, these countries also owe France billions in return. Frances problem is that it is greatly exposed to the eurozones troubled debtors. Its banks hold large amounts of Greek, Italian and Spanish debt. This is causing market turbulence, especially against a backdrop of faltering French growth and low consumer spending.'>
    <owes name='Italy' amount='37.6'/>
    <owes name='Japan' amount='79.8'/>
    <owes name='Germany' amount='123.5'/>
    <owes name='UK' amount='227'/>
    <owes name='US' amount='202.1'/>
  </country>
  <country name='Spain' debt='1900' text='Spain owes large amounts to Germany and France. However, its number one worry is bailed-out Portugal, which is indebted to it by billions of euros. As the country attempts to get its own debts under control, there are fears the country could be thrown back into recession after Novembers parliamentary elections. The bursting of a housing and construction boom in 2008 had plunged Spains economy into a recession deeper than in many other European countries.'>
    <owes name='Portugal' amount='19.7'/>
    <owes name='Italy' amount='22.3'/>
    <owes name='Japan' amount='20'/>
    <owes name='Germany' amount='131.7'/>
    <owes name='UK' amount='74.9'/>
    <owes name='US' amount='49.6'/>
    <owes name='France' amount='112'/>
  </country>
  <country name='Portugal' debt='400' text='Portugal, the third eurozone country to need a bail-out, is in deep recession. It is currently implementing a series of austerity measures as well as planning a series of privatisations to fix its shaky finances and reduce its debt burden. The country is highly indebted to Spain, and its banks are owed 7.5bn euros by Greece.'>
    <owes name='Italy' amount='2.9'/>
    <owes name='Germany' amount='26.6'/>
    <owes name='UK' amount='18.9'/>
    <owes name='US' amount='3.9'/>
    <owes name='France' amount='19.1'/>
    <owes name='Spain' amount='65.7'/>
  </country>
  <country name='Italy' debt='2000' text='Italy has a large amount of debt, but it is a relatively wealthy country compared with Greece and Portugal. However, doubt about Italys leadership and fears that its debt load could grow more quickly than the Italian economys capacity to support it have left the markets jittery. France is most exposed to Italian debt.'>
    <owes name='Japan' amount='32.8'/>
    <owes name='Germany' amount='120'/>
    <owes name='UK' amount='54.7'/>
    <owes name='US' amount='34.8'/>
    <owes name='France' amount='309'/>
    <owes name='Spain' amount='29.5'/>
  </country>
  <country name='Ireland' debt='1700' text='One of three eurozone countries to so far receive a bail-out, Ireland has introduced a series of tough austerity budgets. Its economy is now showing a modest recovery. After the boom years leading up to 2008, the country fell into recession as a result of the global credit squeeze, which ended the supply of cheap credit that had fuelled the unsustainable growth in its housing market. It shows a very high level of gross foreign debt to GDP because, although it is a small country, it has a large financial sector - including a big overseas presence. The UK is Irelands biggest creditor.'>
    <owes name='Japan' amount='15.4'/>
    <owes name='Germany' amount='82'/>
    <owes name='UK' amount='104.5'/>
    <owes name='US' amount='39.8'/>
    <owes name='France' amount='23.8'/>
  </country>
  <country name='Greece' debt='400' text='Greece is heavily indebted to eurozone countries and is one of three eurozone countries to have received a bail-out. Although the Greek economy is small and direct damage of it defaulting on its debts might be absorbed by the eurozone, the big fear is contagion - or that a Greek default could trigger a financial catastrophe for other, much bigger economies, such as Italy.'>
    <owes name='Germany' amount='15.9'/>
    <owes name='UK' amount='9.4'/>
    <owes name='US' amount='6.2'/>
    <owes name='France' amount='41.1'/>
    <owes name='Portugal' amount='7.5'/>
    <owes name='Italy' amount='2.8'/>
  </country>
  <country name='Japan' debt='2000' text='The worlds third-largest economy has the highest public debt level amongst developed economies. However, most of its debt is owed internally, so it is not seen as at risk of default. The global financial crisis, this years earthquake and tsunami, a strong yen and Europes debt crisis are clouding its current economic outlook. But the government has pledged to turn the countrys annual budget deficit into a surplus by 2020.'>
    <owes name='Germany' amount='42.5'/>
    <owes name='UK' amount='101.8'/>
    <owes name='US' amount='244.8'/>
    <owes name='France' amount='107.7'/>
  </country>
  <country name='Germany' debt='4200' text='The biggest European economy owes France, Italy and the US most money. However, these economies also owe Germany billions in return. Regarding its relationship with the troubled eurozone countries, Germany is exposed to Greek, Irish and Portuguese, but mostly, Spanish debt. If any of these defaults, Germany will be hit. Its economy is slowing, mainly because of the problems plaguing its eurozone partners. And as Europes industrial powerhouse, any problems in Germany mean more problems for the eurozone, but also for the wider international system.'>
    <owes name='UK' amount='141.1'/>
    <owes name='US' amount='174.4'/>
    <owes name='France' amount='205.8'/>
    <owes name='Italy' amount='202.7'/>
    <owes name='Japan' amount='108.3'/>
  </country>
  <country name='UK' debt='7300' text='The UK has very large amounts of overseas debt, of which the biggest component is the banking industry. The high debt to GDP ratio is explained by the UKs active financial sector, where there is a great deal of capital movement. This level of overall external debt is generally not seen as a problem because the UK also holds high-value assets. Having said this, the UK economy remains in the doldrums and the country is highly exposed to Irish as well as Italian and Portuguese debt. The UK in turn owes hundreds of billions to Germany and Spain.'>
    <owes name='US' amount='578.6'/>
    <owes name='France' amount='209.9'/>
    <owes name='Spain' amount='316.6'/>
    <owes name='Ireland' amount='113.5'/>
    <owes name='Japan' amount='122.7'/>
    <owes name='Germany' amount='379.3'/>
  </country>
  <country name='US' debt='10900' text='Although the USs overseas debt almost equates to its annual GDP, it is still regarded as a safe bet. However, its credit rating has been downgraded. Although Asia - primarily China and Japan - holds the majority of US debt, Europe has the second largest percentage. This means whatever happens in the eurozone will have a deep impact on the US banking system. Within Europe, the UK, Switzerland and France hold the largest amount of US debt, amounting to hundreds of billions of dollars.'>
    <owes name='France' amount='440.2'/>
    <owes name='Spain' amount='170.5'/>
    <owes name='Japan' amount='835.2'/>
    <owes name='Germany' amount='414.5'/>
    <owes name='UK' amount='834.5'/>
  </country>
</debt>";
  }
}
