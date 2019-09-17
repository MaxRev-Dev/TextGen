(function ()
{    
    var tour = new Shepherd.Tour({
            defaults: {
                classes: 'shepherd-theme-arrows'
            }
        });


        tour.addStep('st-1', {
            title: 'Привіт!',
            text: 'Я покажу Вам декілька простих кроків з генератором',

            buttons: [{
            text: 'Ні, дякую',
            action: tour.cancel
        },{
                text: 'Продовжимо',
                action: tour.next
            }]
        });
        tour.addStep('st-4', {
            title: 'Налаштування',
            text: 'Можна змінити мову, шрифт та шляхи до файлів',
            attachTo: '#loc-prefs-drop bottom',
            buttons: [{
                text: 'Далі',
                action: tour.next
            }]
        });
            tour.addStep('st-5', {
            title: 'Згенероване',
            text: 'Натисніть, щоб побачити згенерований контент',
            attachTo: '#loc-gend right',
classes:'shepherd-theme-arrows st-left',
            buttons: [{
                text: 'Зрозуміло',
                action: tour.next
            }],
            when: {
                'before-show': function () {
                   setTimeout(function () {  $('#tab1-btn').focus();});
                },}
        });
        tour.addStep('st-6', {
            title: 'Редактор',
            text: 'Вкладка з редактором тексту',
            attachTo: '#loc-txt-from right',
classes:'shepherd-theme-arrows st-right',
            buttons: [{
                text: 'Супер!',
                action: tour.next
            }],
            when: {
                'before-show': function () {
                   setTimeout(function () {  $('#tab2-btn').focus();});
                },}
        });
        tour.addStep('st-2', {
            title: 'Добре. Давайте щось відкриємо..',
            text: 'Відкрийте файл з \"маркерами\"',
            attachTo: '#loc-open bottom',
            buttons: [{
                text: 'Ок',
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
            title: 'Чудово! Після цього..',
            text: 'Просто натисніть',
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