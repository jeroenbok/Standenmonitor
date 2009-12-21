
/* SUBMIT SCRIPT */ 



/* FITLERBUILD  */

if (document.getElementById("spnCalendar") == null) {
    document.write('<div id="spnCalendar" onMouseOut="fnStartCalHideDelay()"  onMouseOver="fnCancelCalHideDelay()" style="position:absolute; display:none; width:160px; height:150px; background-color:white;border:1px solid black;z-index:999"></div>');
}

var strDateFormat = new String("d-M-yyyy")
var CurrentFilter = null

/* Enumeration FilterType */
function enum_FilterType() {
    this.String = 1
    this.Date = 2
    this.Integer = 3
    this.StringArray = 4
    this.IntegerArray = 5
    this.DateArray = 6
}
var FilterType = new enum_FilterType()
/* End Enumeration FilterType */

/* Class DocumentFilterItem */
function DocumentFilterItem(strName, rawValue) {
    this.strName = strName
    this.rawValue = rawValue
}

DocumentFilterItem.prototype.fnGetRawFilterValue = function() {
    return this.rawValue
}

DocumentFilterItem.prototype.fnGetFilterValue = function(type) {

    return this.fnParseFilterValue(this.rawValue, type)
}

DocumentFilterItem.prototype.fnParseRawFilterValue = function(value, type) {
    var strReturnValue = new String
    if (value == null)
        return null
    switch (type) {
        case FilterType.String:
            // strReturnValue = escape(value)
            strReturnValue = encodeURI(value)
            break;
        case FilterType.Integer:
            strReturnValue = value
            break;
        case FilterType.Date:
            strReturnValue = getDateFromFormat(value, strDateFormat) / 1000.0
            break;
        case FilterType.StringArray:
            for (var j = 0; j < value.length; j++) {
                if (strReturnValue.length > 0) {
                    strReturnValue += ";"
                }
                strReturnValue += escape(value[j])
            }
            // strReturnValue = escape(strReturnValue)			
            strReturnValue = encodeURI(strReturnValue)
            break;
        case FilterType.IntegerArray:
            for (var j = 0; j < value.length; j++) {
                if (strReturnValue.length > 0) {
                    strReturnValue += ";"
                }
                strReturnValue += value[j]
            }
            // strReturnValue = escape(strReturnValue)
            strReturnValue = encodeURI(strReturnValue)
            break;

        case FilterType.DateArray:
            for (var j = 0; j < value.length; j++) {
                if (strReturnValue.length > 0) {
                    strReturnValue += ";"
                }
                strReturnValue += getDateFromFormat(value[j], strDateFormat) / 1000.0
            }
            // strReturnValue = escape(strReturnValue)	
            strReturnValue = encodeURI(strReturnValue)
            break;

    }
    return strReturnValue
}


DocumentFilterItem.prototype.fnParseFilterValue = function(rawValue, type) {
    var returnValue
    switch (type) {
        case FilterType.String:
            // returnValue = unescape(rawValue)
            returnValue = decodeURI(rawValue).replace(/%2B/, "+")
            break;
        case FilterType.Integer:
            returnValue = rawValue
            break;
        case FilterType.Date:
            returnValue = formatDate(new Date(rawValue * 1000.0), strDateFormat)
            break;
        case FilterType.StringArray:
            strRawArray = decodeURI(rawValue).replace(/%2B/, "+")
            // strRawArray = unescape(rawValue)
            valueArr = rawValue.split(";")
            for (var j = 0; j < valueArr.length; j++) {

                valueArr[j] = unescape(valueArr[j])
            }
            returnValue = valueArr
            break;
        case FilterType.IntegerArray:
            strRawArray = decodeURI(rawValue).replace(/%2B/, "+")
            // strRawArray = unescape(rawValue)
            valueArr = rawValue.split(";")
            returnValue = valueArr
            break;
        case FilterType.DateArray:
            strRawArray = decodeURI(rawValue).replace(/%2B/, "+")
            // strRawArray = unescape(rawValue)
            valueArr = rawValue.split(";")
            for (var j = 0; j < valueArr.length; j++) {

                valueArr[i] = formatDate(new Date(valueArr[i] * 1000.0), strDateFormat)
            }
            returnValue = valueArr
            break;

    }
    return returnValue
}

/* End Class DocumentFilterItem */

/* Class DocumentFilter */
function DocumentFilter(strBaseUrl) {
    this.filterItems = new Array()
    this.strBaseUrl = strBaseUrl
}

DocumentFilter.prototype.fnSetFilterItem = function(strName, type, value) {
    var filterItemIndex = this.fnGetFilterItemIndex(strName)
    if (filterItemIndex < 0) {
        filterItemIndex = this.filterItems.length
    }

    var rawValue = DocumentFilterItem.prototype.fnParseRawFilterValue(value, type)
    this.filterItems[filterItemIndex] = new DocumentFilterItem(strName, rawValue)
}

DocumentFilter.prototype.fnSetRawFilterItem = function(strName, value) {
    var filterItemIndex = this.fnGetFilterItemIndex(strName)
    if (filterItemIndex < 0) {
        filterItemIndex = this.filterItems.length
    }
    this.filterItems[filterItemIndex] = new DocumentFilterItem(strName, value)
}

DocumentFilter.prototype.fnSetFilterItemObject = function(DocumentFilterItem) {
    var filterItemIndex = this.fnGetFilterItemIndex(DocumentFilterItem.strName)
    if (filterItemIndex < 0) {
        filterItemIndex = this.filterItems.length
    }
    this.filterItems[filterItemIndex] = DocumentFilterItem
}

DocumentFilter.prototype.fnGetFilterItem = function(strName) {
    var filterItemIndex = this.fnGetFilterItemIndex(strName)
    if (filterItemIndex < 0) {
        return null
    }
    return this.filterItems[filterItemIndex]
}

DocumentFilter.prototype.fnGetFilterItemIndex = function(strName) {
    for (var i = 0; i < this.filterItems.length; i++) {
        if (this.filterItems[i].strName == strName)
            return i
    }
    return -1
}


DocumentFilter.prototype.fnGetFilterHash = function() {
    var strHash = new String()
    for (var i = 0; i < this.filterItems.length; i++) {
        fItem = this.filterItems[i]
        if (fItem.fnGetRawFilterValue() != null) {
            if (strHash.length > 0) {
                strHash += "&"
            }

            strHash += fItem.strName
            strHash += "="
            strHash += fItem.fnGetRawFilterValue()
        }
    }
    return encodeBase64(strHash)
}

DocumentFilter.prototype.fnGetTargetUrl = function() {

    var strTargetUrl = new String(this.strBaseUrl)
    if (strTargetUrl.indexOf("?") >= 0) {
        strTargetUrl += "&strFilter=" + this.fnGetFilterHash()
    }
    else {
        strTargetUrl += "?strFilter=" + this.fnGetFilterHash()
    }
    return strTargetUrl
}

DocumentFilter.prototype.fnExecuteFilter = function() {
    window.location.href = this.fnGetTargetUrl()
}


/* End Class DocumentFilter */

function fnSelectTextbox(textboxRef, strDefaultValue) {
    if (textboxRef.value == strDefaultValue) {

        textboxRef.value = ""
        //set cursor in first place
        if (textboxRef.createTextRange) {
            var r = textboxRef.createTextRange();
            r.moveStart('character', textboxRef.value.length);
            r.collapse();
            r.select();
        }

    }
}

function fnDeSelectTextbox(textboxRef, strDefaultValue) {
    if (textboxRef.value == "") {
        textboxRef.value = strDefaultValue
    }
}

function fnShowCalendar(evt, refButton, strTextboxRef, offsetX, offsetY) {

    document.getElementById("spnCalendar").setAttribute("strTextboxRef", strTextboxRef)

    var textboxRef = document.getElementById(document.getElementById("spnCalendar").getAttribute("strTextboxRef"))
    var strValue = textboxRef.value
    var curDate = null
    if (isDate(strValue, strDateFormat))
        curDate = new Date(getDateFromFormat(strValue, strDateFormat))

    document.getElementById("spnCalendar").setAttribute("dtSelectedDate", curDate)
    if (curDate == null)
        curDate = new Date()

    fnBuildDateSelector(curDate.getMonth() + 1, curDate.getFullYear())

    document.getElementById("spnCalendar").style.display = "block"



    var posx
    var posy
    if (evt.pageX || evt.pageY) {
        posx = evt.pageX;
        posy = evt.pageY;
    }
    else if (evt.clientX || evt.clientY) {
        posx = evt.clientX + document.documentElement.scrollLeft;
        posy = evt.clientY + document.documentElement.scrollTop;
    }
    posx -= document.getElementById("spnCalendar").offsetWidth
    posx += 10
    posy -= 10
    if (!offsetX)
        offsetX = 0
    if (!offsetY)
        offsetY = 0
    document.getElementById("spnCalendar").style.left = (posx + offsetX) + 'px'
    document.getElementById("spnCalendar").style.top = (posy + offsetY) + 'px'
}

function fnBuildDateSelector(intMonth, intYear) {
    var refControl = document.getElementById("spnCalendar")
    var curDate = new Date()

    var strHtml = ""

    var selectedDate = new Date(Date.parse(document.getElementById("spnCalendar").getAttribute("dtSelectedDate")))
    curDate.setDate(1)

    intMonth--
    if (intMonth < 0) {
        intMonth = 11
    }
    var intNextMonth = intMonth + 1
    if (intNextMonth == 12) {

        intNextMonth = 0
    }
    curDate.setMonth(intMonth)
    curDate.setYear(intYear)


    while (curDate.getDay() != 1) {
        curDate = SubtractDay(curDate)
    }

    strHtml = "<table style=\"width:160px\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">"
    strHtml += "<tr  height=\"20\">"
    strHtml += "<td style=\"padding-left:10px;\">"
    var intPrevYear = intYear
    var intPrevMonth = intMonth

    if (intPrevMonth == 0) {
        intPrevMonth = 12
        intPrevYear--
    }
    strHtml += " <a href=\"#\" onclick=\"javascript:fnBuildDateSelector(" + (intPrevMonth) + ", " + intPrevYear + ");return false;\">&lt;&lt;</a>"
    strHtml += "</td>"
    strHtml += "<td align=\"center\">"
    strHtml += calMonthNames[intMonth] + " " + intYear
    strHtml += "</td>"
    strHtml += "<td align=\"right\" style=\"padding-right:10px;\">"
    var intNextYear = intYear
    if (intNextMonth == 0)
        intNextYear++

    strHtml += " <a href=\"#\" onclick=\"javascript:fnBuildDateSelector(" + (intNextMonth + 1) + ", " + intNextYear + ");return false;\">&gt;&gt;</a>"
    strHtml += "</td>"
    strHtml += "</tr>"
    strHtml += "</table>"


    strHtml += "<table style=\"width:160px\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">"
    strHtml += "<tr  height=\"20\">"
    strHtml += " <th align=\"center\" valign=\"middle\" width=\"15\" >&nbsp;</th>";
    for (var i = 0; i < calDayNames.length; i++) {

        strHtml += " <th align=\"center\" valign=\"middle\">" + calDayNames[i].substring(0, 1) + "</th>"
    }
    strHtml += "</tr>"


    while (curDate.getMonth() != intNextMonth) {
        strHtml += "<tr height=\"20\">"
        strHtml += " <th align=\"center\" valign=\"middle\" class=\"bold_Calender\">" + getWeekNr(curDate) + "</th>"
        for (i = 0; i < 7; i++) {


            var strDate = curDate.getDate()
            var strClickEvent = "fnSelectCalendarDate(" + strDate + "," + (curDate.getMonth() + 1) + "," + (curDate.getFullYear()) + ")"
            if (selectedDate) {
                if (selectedDate.getFullYear() == curDate.getFullYear()) {
                    if (selectedDate.getMonth() == curDate.getMonth()) {
                        if (selectedDate.getDate() == curDate.getDate()) {
                            strDate = "<strong>" + curDate.getDate() + "</strong>"
                        }

                    }
                }
            }

            //if (curDate.getMonth() == intMonth)

            strHtml += "<td align=\"center\" valign=\"middle\" style=\"cursor:hand;cursor:pointer;\" onclick=\"" + strClickEvent + "\">"
            strHtml += strDate
            strHtml += "</td>"
            curDate = AddDay(curDate)
        }
        strHtml += "</tr>"
    }
    strHtml += "</table>"

    /*
    refHeader = document.getElementById("evt_calendar_header")
    refHeader.innerHTML = monthNames[intMonth] + " " + intYear
    refHeader.setAttribute("intMonth", intMonth+1)
    refHeader.setAttribute("intYear", intYear)
    */
    refControl.innerHTML = strHtml
}


function fnSelectCalendarDate(intDate, intMonth, intYear) {
    var strTextboxref = document.getElementById("spnCalendar").getAttribute("strTextboxRef")
    var selectedDate = new Date(0)

    selectedDate.setFullYear(intYear, intMonth - 1, intDate)

    document.getElementById(strTextboxref).value = formatDate(selectedDate, strDateFormat)


    document.getElementById("spnCalendar").style.display = "none"
}

var calLayerTimer
function fnStartCalHideDelay() {
    calLayerTimer = setTimeout("fnCalHide()", 500)
}

function fnCancelCalHideDelay() {
    clearTimeout(calLayerTimer)
}

function fnCalHide() {
    document.getElementById('spnCalendar').style.display = 'none'
}