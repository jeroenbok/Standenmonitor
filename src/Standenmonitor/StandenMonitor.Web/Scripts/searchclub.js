

/*
Submit
R = uitslagen
P = programma
M = schema
FT = vrije teams
*/

function fnCheckAndSubmitCompetition(strModulePart) {

    newFilter = new DocumentFilter(CurrentFilter.strBaseUrl)

    var strModuleName = ''
    if (chkClubMode.checked) {
        newFilter.fnSetFilterItem('ClubMode', FilterType.String, "True")
        strModuleName = 'C' + strModulePart

        // Button ResultMatrix is Standing in Club modi
        if (strModulePart == 'M') //Matrix
            strModuleName = 'CS' //ClubStanding
    } else {
        strModuleName = strModuleName + strModulePart
    }



    newFilter.fnSetFilterItem('M', FilterType.String, strModuleName)

    if ((CurrentFilter.fnGetFilterItem('Club') != null) && (CurrentFilter.fnGetFilterItem('Team') != null)) {
        if (CurrentFilter.fnGetFilterItem('Club').fnGetFilterValue(FilterType.String) == lstDdlClubs.value) {
            newFilter.fnSetFilterItem('Team', FilterType.String, CurrentFilter.fnGetFilterItem('Team').fnGetFilterValue(FilterType.String))
        }
    }

    newFilter.fnSetFilterItem('Club', FilterType.String, lstDdlClubs.value)
    newFilter.fnSetFilterItem("Kind", FilterType.String, lstDdlCompetitionKind.value)
    newFilter.fnSetFilterItem('Age', FilterType.String, lstDdlAgeClassCode.value)
    newFilter.fnSetFilterItem('Sex', FilterType.String, lstDdlTeamSex.value)

    var EindDatum
    var StartDatum

    if (strModulePart == 'P') {
        StartDatum = new Date()
        StartDatum = new Date(StartDatum.getFullYear(), StartDatum.getMonth(), StartDatum.getDate(), 0, 0, 0, 0)

        EindDatum = new Date(StartDatum.getTime() + (8 * 24 * 60 * 60 * 1000))
    } else {
        EindDatum = new Date()
        EindDatum = new Date(EindDatum.getFullYear(), EindDatum.getMonth(), EindDatum.getDate(), 0, 0, 0, 0)

        StartDatum = new Date(EindDatum.getTime() - (8 * 24 * 60 * 60 * 1000))
    }
    //di: DateMin, da: DateMax
    newFilter.fnSetFilterItem('di', FilterType.Date, StartDatum.getDate() + "-" + (1 + StartDatum.getMonth()) + "-" + StartDatum.getFullYear())
    newFilter.fnSetFilterItem('da', FilterType.Date, EindDatum.getDate() + "-" + (1 + EindDatum.getMonth()) + "-" + EindDatum.getFullYear())

    if (chkPref.checked) {
        if (newFilter.strBaseUrl.indexOf("?") >= 0) {
            newFilter.strBaseUrl += "&btSaveCookie=1"
        }
        else {
            newFilter.strBaseUrl += "?btSaveCookie=1"
        }
    }

    fnExecuteCustomFilter(newFilter);

}

/*
Common
*/

function fnClearItemList(lstTarget, boolDisabled, strFirstItem) {
    while (lstTarget.options.length > 1) {
        lstTarget.removeChild(lstTarget.options[lstTarget.options.length - 1])
    }
    lstTarget.options[0].innerHTML = strFirstItem
    lstTarget.disabled = boolDisabled
}

/*
Form logic
*/

//Search button
function fnControlSearchButton() {
    var checkObject
    if (chkClubMode.checked) {
        checkObject = lstDdlClubs
    } else {
        checkObject = lstDdlTeamSex
    }
    if (checkObject.value == '-1') {
        document.getElementById('btnCompetitionResult').disabled = true
        document.getElementById('btnCompetitionSchedule').disabled = true
        document.getElementById('btnCompetitionResultMatrix').disabled = true
        var btnShowFreeTeams = document.getElementById('btnShowFreeTeams')
        if (btnShowFreeTeams) {
            btnShowFreeTeams.disabled = true
        }
    } else {
        document.getElementById('btnCompetitionResult').disabled = false
        document.getElementById('btnCompetitionSchedule').disabled = false
        document.getElementById('btnCompetitionResultMatrix').disabled = false
        var btnShowFreeTeams = document.getElementById('btnShowFreeTeams')
        if (btnShowFreeTeams) {
            btnShowFreeTeams.disabled = (false || (chkClubMode.checked))
        }

        if (lstDdlCompetitionKind.value == 'Y') {
            document.getElementById('btnCompetitionResult').onclick = fnShowAlert
            document.getElementById('btnCompetitionResultMatrix').onclick = fnShowAlert
        } else {
            document.getElementById('btnCompetitionResult').onclick = fnShowCompetitionResult
            document.getElementById('btnCompetitionResultMatrix').onclick = fnShowCompetitionResultMatrix
        }

    }
}



function fnShowCompetitionResult() {
    fnCheckAndSubmitCompetition('R')
}

function fnShowCompetitionResultMatrix() {
    fnCheckAndSubmitCompetition('M')
}

function fnShowAlert() {
    alert('Bij de Jongste Jeugd worden geen uitslagen en standen doorgegeven. Het spelplezier wordt van groter belang geacht dan het competitie-element.')
    return false
}

//Club Change

function fnCompClubChange(strNewValue) {
    if (chkClubMode.checked) {
        document.getElementById('btnCompetitionResult').value = 'uitslagen'
        document.getElementById('btnCompetitionResultMatrix').value = 'standen'
        fnControlSearchButton()
    } else {
        document.getElementById('btnCompetitionResult').value = 'uitslagen en standen'
        document.getElementById('btnCompetitionResultMatrix').value = 'schema'
        fnClearItemList(lstDdlAgeClassCode, true, "- Kies Doelgroep -")
        fnClearItemList(lstDdlTeamSex, true, "- Kies M/V -")
        if (strNewValue != '-1') {
            fnClearItemList(lstDdlCompetitionKind, true, "Bezig met laden...")
            Aspacts.Websites.KNHB.Components.Sportlink.Competition.CompetitionKind.GetCompetitionKinds(strNewValue, strCurrentSportId, fnCompClubChangeCallBack, strNewValue)
        } else {
            fnClearItemList(lstDdlCompetitionKind, true, " - Kies Soort - ")
            fnControlSearchButton()
        }
    }
}

function fnCompClubChangeCallBack(response) {
    if (response.error != null) {
        alert(response.error.Message)
        return
    }

    if (lstDdlClubs && lstDdlClubs.value == response.context) {
        fnClearItemList(lstDdlCompetitionKind, true, " - Kies Soort -")

        for (var i = 0; i < response.value.length; i++) {
            var item = document.createElement("option")
            item.value = response.value[i].CompetitionKindId
            item.text = response.value[i].CompetitionKindDescription
            lstDdlCompetitionKind.options[lstDdlCompetitionKind.options.length] = item;
        }
        if (lstDdlCompetitionKind.options.length == 1)
            fnClearItemList(lstDdlCompetitionKind, true, "geen informatie")
        else
            lstDdlCompetitionKind.disabled = false
        if (lstDdlCompetitionKind.options.length == 2) {
            lstDdlCompetitionKind.value = lstDdlCompetitionKind.options[1].value
            fnCompKindChange(lstDdlCompetitionKind.value)
        } else {
            fnControlSearchButton()
        }
    }
}

//Kind Change

function fnCompKindChange(strNewValue) {
    fnClearItemList(lstDdlTeamSex, true, "- Kies M/V -")
    if (strNewValue != '-1') {
        fnClearItemList(lstDdlAgeClassCode, true, "Bezig met laden...")
        var context = new Array()
        context[0] = lstDdlCompetitionKind.value
        context[1] = lstDdlClubs.value
        Aspacts.Websites.KNHB.Components.Sportlink.Competition.AgeClassCode.GetAgeClassCodes(lstDdlClubs.value, strCurrentSportId, strNewValue, fnCompKindChangeCallBack, context)
    } else {
        fnClearItemList(lstDdlAgeClassCode, true, " - Kies Klasse -")
        fnControlSearchButton()
    }
}

function fnCompKindChangeCallBack(response) {
    if (response.error != null) {
        alert(response.error.Message)
        return
    }

    if ((lstDdlClubs.value == response.context[1]) && (lstDdlCompetitionKind.value == response.context[0])) {
        fnClearItemList(lstDdlAgeClassCode, true, " - Kies Doelgroep -")

        for (var i = 0; i < response.value.length; i++) {
            var item = document.createElement("option")
            item.value = response.value[i].AgeClassCodeId
            item.text = response.value[i].AgeClassCodeDescription
            lstDdlAgeClassCode.options[lstDdlAgeClassCode.options.length] = item;
        }
        if (lstDdlAgeClassCode.options.length == 1)
            fnClearItemList(lstDdlAgeClassCode, true, "geen informatie")
        else
            lstDdlAgeClassCode.disabled = false
        if (lstDdlAgeClassCode.options.length == 2) {
            lstDdlAgeClassCode.value = lstDdlAgeClassCode.options[1].value
            fnCompAgeClassCodeChange(lstDdlAgeClassCode.value)
        } else {
            fnControlSearchButton()
        }
    }
}

//AgeClassCode Change

function fnCompAgeClassCodeChange(strNewValue) {
    if (strNewValue != '-1') {
        fnClearItemList(lstDdlTeamSex, true, "Bezig met laden...")
        var context = new Array()
        context[0] = lstDdlCompetitionKind.value
        context[1] = lstDdlClubs.value
        context[2] = lstDdlAgeClassCode.value
        Aspacts.Websites.KNHB.Components.Sportlink.Competition.TeamSex.GetTeamSex(lstDdlClubs.value, strCurrentSportId, lstDdlCompetitionKind.value, strNewValue, fnCompAgeClassCodeChangeCallBack, context)
    } else {
        fnClearItemList(lstDdlTeamSex, true, " - Kies M/V -")
        fnControlSearchButton()
    }
}

function fnCompAgeClassCodeChangeCallBack(response) {
    if (response.error != null) {
        alert(response.error.Message)
        return
    }

    if ((lstDdlClubs.value == response.context[1]) && (lstDdlCompetitionKind.value == response.context[0]) && (lstDdlAgeClassCode.value == response.context[2])) {
        fnClearItemList(lstDdlTeamSex, true, " - Kies M/V -")

        for (var i = 0; i < response.value.length; i++) {
            var item = document.createElement("option")
            item.value = response.value[i].TeamSexId
            item.text = response.value[i].TeamSexDescription
            lstDdlTeamSex.options[lstDdlTeamSex.options.length] = item;
        }
        if (lstDdlTeamSex.options.length == 1)
            fnClearItemList(lstDdlTeamSex, true, "geen informatie")
        else
            lstDdlTeamSex.disabled = false
        if (lstDdlTeamSex.options.length == 2) {
            lstDdlTeamSex.value = lstDdlTeamSex.options[1].value
            fnCompTeamSexChange(lstDdlTeamSex.value)
        } else {
            fnControlSearchButton()
        }
    }
}

//TeamSex

function fnCompTeamSexChange(strNewValue) {
    fnControlSearchButton()
}

//Clubmode

function fnClickClubMode() {
    if (chkClubMode.checked) {
        fnClearItemList(lstDdlAgeClassCode, true, "- Kies Doelgroep -")
        fnClearItemList(lstDdlTeamSex, true, "- Kies M/V -")
        fnClearItemList(lstDdlCompetitionKind, true, " - Kies Soort - ")
    }
    fnCompClubChange(lstDdlClubs.value)
}

/*
Startup actions
*/

if (chkClubMode.checked) {
    document.getElementById('btnCompetitionResult').value = 'uitslagen'
    document.getElementById('btnCompetitionResultMatrix').value = 'standen'
    fnControlSearchButton()
} else {
    document.getElementById('btnCompetitionResult').value = 'uitslagen en standen'
    document.getElementById('btnCompetitionResultMatrix').value = 'schema'
}
fnControlSearchButton()

/*
function createCookie(name,value,days) {
if (days) {
var date = new Date();
date.setTime(date.getTime()+(days*24*60*60*1000));
var expires = "; expires="+date.toGMTString();
}
else var expires = "";
var ck = name+"="+value+expires+"; path=/";
document.cookie = name+"="+value+expires+"; path=/";
}
*/

function readCookie(name) {
    var nameEQ = name + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}

if (CurrentFilter.filterItems.length == 0) {
    var strCookieValue = readCookie("strCompEngine3_" + strCurrentSportId)
    if (strCookieValue != null) {
        var strTargetUrl = new String(CurrentFilter.strBaseUrl)
        if (strTargetUrl.indexOf("?") >= 0) {
            strTargetUrl += "&strFilter=" + strCookieValue
        }
        else {
            strTargetUrl += "?strFilter=" + strCookieValue
        }
        location.href = strTargetUrl
    }
} 

