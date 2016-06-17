<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    xmlns:s="http://schemas.microsoft.com/VisualStudio/2005/CodeSnippet"
    exclude-result-prefixes="msxsl">
  <xsl:output method="text"/>

  <xsl:template match="/">
using System;

namespace Snippets
{
    <xsl:apply-templates select="//s:CodeSnippet"/>
}
  </xsl:template>

  <!-- matches a CodeSnippet element generating an attribute -->
  <xsl:template match="s:CodeSnippet">
    /// &lt;summary&gt;
    /// <xsl:value-of select="s:Header/s:Description"/>
    /// &lt;/summary&gt;
    [AttributeUsage(AttributeTargets.Class , AllowMultiple = true)]
    public class Snippet<xsl:value-of select="s:Header/s:Shortcut"/>  : Attribute
    {
    <!-- generate the attribute properties -->
    <xsl:apply-templates select="//s:Declarations/s:Literal"/>
    <!-- add the snippet code to the attribute -->
    <xsl:apply-templates select="//s:Code"/>
    }
  </xsl:template>

  <!-- generates a string property for codesnippet literal -->
  <xsl:template match="s:Literal">
        /// &lt;summary&gt;
        /// <xsl:value-of select="s:ToolTip"/>
        /// &lt;/summary&gt;
        public string <xsl:value-of select="s:ID"/> = "<xsl:value-of select="s:Default"/>";
  </xsl:template>

  <xsl:template match="s:Code">
    <!-- escape any quotes in the snippet -->
    <xsl:variable name="escaped">
      <xsl:call-template name="escapeQuot">
        <xsl:with-param name="text" select="."/>
      </xsl:call-template>
    </xsl:variable>

    <!-- add a method that returns the snippet -->
    /// &lt;summary&gt;
    /// Gets the code snippet
    /// &lt;/summary&gt;
    public string GetSnippet()
    {
    return @"<xsl:value-of select="$escaped" />";
    }
  </xsl:template>


  <!-- relpaces single quotes with double-quotes -->
  <xsl:template name="escapeQuot">
    <xsl:param name="text"/>
    <xsl:choose>
      <xsl:when test="contains($text, '&quot;')">
        <xsl:variable name="bufferBefore" select="substring-before($text,'&quot;')"/>
        <xsl:variable name="newBuffer" select="substring-after($text,'&quot;')"/>
        <xsl:value-of select="$bufferBefore"/>
        <xsl:text>""</xsl:text>
        <xsl:call-template name="escapeQuot">
          <xsl:with-param name="text" select="$newBuffer"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$text"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>
