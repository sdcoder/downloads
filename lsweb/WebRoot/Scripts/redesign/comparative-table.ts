class Competitor {
    name: string;
    competitorType: string;
    benefits: Array<boolean>;
    constructor(competitorName: string, typeOfCompetitor: string, benefitsOffered: Array<boolean>) {
        this.name = competitorName;
        this.competitorType = typeOfCompetitor;
        this.benefits = benefitsOffered;
    }
}

$(document).ready(function () {
    let discover: Competitor = new Competitor('Discover', 'Bank', [false, true, false, true, false, false, true]);
    let lendingClub: Competitor = new Competitor('Lending Club', 'Online Lender', [false, false, false, true, false, false, false]);

    let leftArrow: any = $('#comparative-left-arrow');
    let rightArrow: any = $('#comparative-right-arrow');

    let secondCompetitor: any = $('#second-competitor');
    let competitorType: any = $('#competitor-type');

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

    let capitalOneAuto: Competitor = new Competitor('CapitalOne Auto', 'Bank', [false, true, false, true, false, false]);
    let penFed: Competitor = new Competitor('PenFed', 'Credit Union', [false, false, false, true, true, true]);
    
    let leftArrowAuto: any = $('#comparative-left-arrow-auto');
    let rightArrowAuto: any = $('#comparative-right-arrow-auto');

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

    function loadCompetitor(competitor: Competitor) {
        secondCompetitor.text(competitor.name);
        competitorType.text(competitor.competitorType);
        for (var i = 0; i < competitor.benefits.length; i++) {
            let hasBenefit = competitor.benefits[i];
            let td = $('#comparative-table-mobile > tbody > tr:nth-child(' + (i + 1) + ') > td:nth-child(3)');
            td.html(hasBenefit ? '<i class="ls ls-check ls-3x"></i>' : '');
        }
    }
});