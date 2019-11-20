var Competitor = /** @class */ (function () {
    function Competitor(competitorName, typeOfCompetitor, benefitsOffered) {
        this.name = competitorName;
        this.competitorType = typeOfCompetitor;
        this.benefits = benefitsOffered;
    }
    return Competitor;
}());
$(document).ready(function () {
    var discover = new Competitor('Discover', 'Bank', [false, true, false, true, false, false, true]);
    var lendingClub = new Competitor('Lending Club', 'Online Lender', [false, false, false, true, false, false, false]);
    var leftArrow = $('#comparative-left-arrow');
    var rightArrow = $('#comparative-right-arrow');
    var secondCompetitor = $('#second-competitor');
    var competitorType = $('#competitor-type');
    leftArrow.click(function () {
        if (!leftArrow.hasClass('disabled')) {
            leftArrow.addClass('disabled');
            rightArrow.removeClass('disabled');
            loadCompetitor(discover);
        }
    });
    rightArrow.click(function () {
        if (!rightArrow.hasClass('disabled')) {
            rightArrow.addClass('disabled');
            leftArrow.removeClass('disabled');
            loadCompetitor(lendingClub);
        }
    });
    var capitalOneAuto = new Competitor('CapitalOne Auto', 'Bank', [false, true, false, true, false, false]);
    var penFed = new Competitor('PenFed', 'Credit Union', [false, false, false, true, true, true]);
    var leftArrowAuto = $('#comparative-left-arrow-auto');
    var rightArrowAuto = $('#comparative-right-arrow-auto');
    leftArrowAuto.click(function () {
        if (!leftArrowAuto.hasClass('disabled')) {
            leftArrowAuto.addClass('disabled');
            rightArrowAuto.removeClass('disabled');
            loadCompetitor(capitalOneAuto);
        }
    });
    rightArrowAuto.click(function () {
        if (!rightArrowAuto.hasClass('disabled')) {
            rightArrowAuto.addClass('disabled');
            leftArrowAuto.removeClass('disabled');
            loadCompetitor(penFed);
        }
    });
    function loadCompetitor(competitor) {
        secondCompetitor.text(competitor.name);
        competitorType.text(competitor.competitorType);
        for (var i = 0; i < competitor.benefits.length; i++) {
            var hasBenefit = competitor.benefits[i];
            var td = $('#comparative-table-mobile > tbody > tr:nth-child(' + (i + 1) + ') > td:nth-child(3)');
            td.html(hasBenefit ? '<i class="ls ls-check ls-3x"></i>' : '');
        }
    }
});
//# sourceMappingURL=comparative-table.js.map