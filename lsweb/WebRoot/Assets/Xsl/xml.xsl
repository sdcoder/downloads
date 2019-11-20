<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <!-- Match the root node -->
  <xsl:output method="xml" indent="yes" encoding="UTF-16" omit-xml-declaration="yes"/>
  <xsl:template match="/">
    <DIV STYLE="font-family:Arial; font-size:13pt; font-weight: bold; margin-bottom:2em">
      <xsl:apply-templates select="*|@*|node()|comment()|processing-instruction()"/>
    </DIV>
  </xsl:template>

  <xsl:template match="*">
    <DIV STYLE="margin-left:1em; color:red">&lt;<xsl:value-of select="name()"/><xsl:apply-templates select="@*"/>/&gt;</DIV>
  </xsl:template>

  <xsl:template match="node()">
    <DIV STYLE="margin-left:1em">
      <SPAN STYLE="color:red">&lt;<xsl:value-of select="name()"/><xsl:apply-templates select="@*"/>&gt;</SPAN>
      <xsl:apply-templates select="node()"/>
      <SPAN STYLE="color:red">&lt;/<xsl:value-of select="name()"/>&gt;
      </SPAN>
    </DIV>
  </xsl:template>

  <xsl:template match="@*">
    <SPAN STYLE="color:blue">
      <xsl:value-of select="concat(' ', name())"/>="<xsl:value-of select="."/>"
    </SPAN>
  </xsl:template>

  <xsl:template match="comment()">
    <DIV STYLE="margin-left:1em; color:gray">
      &lt;!--<xsl:value-of select="name()"/>--?&gt;
    </DIV>
  </xsl:template>

  <xsl:template match="processing-instruction()">
    <DIV STYLE="margin-left:1em; color:purple">
      &lt;?<xsl:value-of select="name()"/><xsl:apply-templates select="@*"/>?&gt;
    </DIV>
  </xsl:template>

  <xsl:template match="text()">
    <xsl:value-of select="."/>
  </xsl:template>

</xsl:stylesheet>
<!-- Stylus Studio meta-information - (c) 2004-2005. Progress Software Corporation. All rights reserved.
<metaInformation>
<scenarios/><MapperMetaTag><MapperInfo srcSchemaPathIsRelative="yes" srcSchemaInterpretAsXML="no" destSchemaPath="" destSchemaRoot="" destSchemaPathIsRelative="yes" destSchemaInterpretAsXML="no"/><MapperBlockPosition><template match="/"></template></MapperBlockPosition></MapperMetaTag>
</metaInformation>
-->