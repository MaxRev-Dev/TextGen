(function () {
    var tour = new Shepherd.Tour({
        defaults: {
            classes: 'shepherd-theme-arrows'
        }
    });


    tour.addStep('st-1', {
        title: 'Hi!',
        text: 'I will show you some simple steps in generating content',

        buttons: [{
            text: 'No, thanks',
            action: tour.cancel
        }, {
            text: 'Let`s start!',
            action: tour.next
            }]
    });
    tour.addStep('st-4', {
        title: 'Settings',
        text: 'You can change here language settings and input font size',
        attachTo: '#loc-prefs-drop bottom',
        buttons: [{
            text: 'Next',
            action: tour.next
            }]
    });
    tour.addStep('st-5', {
        title: 'Generated',
        text: 'Click to see generated content',
        attachTo: '#loc-gend right',
        classes: 'shepherd-theme-arrows st-left',
        buttons: [{
            text: 'Next',
            action: tour.next
            }],
        when: {
            'before-show': function () {
                setTimeout(function () {
                    $('#tab1-btn').focus();
                });
            },
        }
    });
    tour.addStep('st-6', {
        title: 'File editor',
        text: 'Click to see and edit your loaded file',
        attachTo: '#loc-txt-from right',
        classes: 'shepherd-theme-arrows st-right',
        buttons: [{
            text: 'Next',
            action: tour.next
            }],
        when: {
            'before-show': function () {
                setTimeout(function () {
                    $('#tab2-btn').focus();
                });
            },
        }
    });
    tour.addStep('st-2', {
        title: 'Ok. Lets open something',
        text: 'Open a special file with `markers`',
        attachTo: '#loc-open bottom',
        buttons: [{
            text: 'Next',
            action: tour.next
            }],
        when: {
            'before-show': function () {
                $('#dp1').dropdown('toggle');
            },
            'show': function () {
                setTimeout(function () {
                    $('#dpw').addClass('show');
                }, 0);
            }
        }
    });

    tour.addStep('st-3', {
        title: 'Good! And now...',
        text: 'Just click',
        attachTo: '#loc-gen top',
        buttons: false,
        when: {
            'show': function () {
                setTimeout(function () {
                    $('body').on('click', function () {
                        tour.complete();
                    });
                }, 300);
            }
        }
    });

    tour.start();

})();
