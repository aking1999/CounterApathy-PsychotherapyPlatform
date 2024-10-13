var allCount;
var unreadCount;

function GetAllNotifications() {
    $.ajax({
        url: url + "GetAll",
        method: "GET",
        success: function (result) {
            allCount = result.allCount;
            unreadCount = result.unreadCount;

            SetUnreadNotificationCounter(unreadCount);

            if (allCount === 0) {
                $('#notification-center-content').append('<div class="card"><div class="card-body"><span id="no-new-notification"><center class="text-secondary"><i class="far fa-thumbs-up text-primary mr-2"></i>Nema novih notifikacija</center></span></div></div>');
            } else {
                $('#no-new-notification').remove();

                /*SetUnreadNotificationCounter(unreadCount);*/

                var notifications = result.userNotification;
                $('#notifications-holder').nextAll('aleksajecar2').remove();
                notifications.forEach(element => {
                    CreateNotificationDropdownElement(element);
                });
            }
        },
        error: function () {
            HandleError('Došlo je do greške', 'Molimo pokušajte ponovo ili kontaktirajte korisničku podršku.', 'error');
        }
    });
}

function CreateNotificationDropdownElement(element) {
    let notificationsHolder = $('#notifications-holder').clone();
    notificationsHolder.attr('id', 'notifications-holder-' + element.id);
    //notificationsHolder.attr('href', 'https://google.com');

    let notificationListMain = notificationsHolder.find('#notification-list-main');
    notificationListMain.attr('id', 'notification-list-' + element.id);

    let checkIcon = notificationsHolder.find('#check-icon-main');
    checkIcon.attr('id', 'check-icon-' + element.id);

    let read = notificationsHolder.find('#read-main');
    read.attr('id', 'read-' + element.id);

    let paragraphMarkRead = notificationsHolder.find('#paragraph-mark-read-main');
    paragraphMarkRead.attr('id', 'paragraph-mark-read-' + element.id);

    let importantStar = notificationsHolder.find('#important-main');
    importantStar.attr('id', 'important-' + element.id);


    if (element.important) {
        importantStar.removeClass('fal');
        importantStar.addClass('fas');
        importantStar.addClass('important-star--true');
    }
    else {
        importantStar.removeClass('fas');
        importantStar.addClass('fal');
        importantStar.removeClass('important-star--true');
    }

    importantStar.click(function () {
        importantStar.toggleClass('fas');
        importantStar.toggleClass('fal');
        importantStar.toggleClass('important-star--true')

        if (element.important === true) {
            $.ajax({
                url: url + "SetUnimportant",
                method: "POST",
                data: { notificationId: element.id },
                success: function () {
                    toastr['success']('', 'Notifikacija je uklonjena iz bitnih');
                    element.important = false;
                },
                error: function () {
                    HandleError('Došlo je do greške', 'Molimo pokušajte ponovo ili kontaktirajte korisničku podršku.', 'error');
                }
            });
        } else {
            $.ajax({
                url: url + "SetImportant",
                method: "POST",
                data: { notificationId: element.id },
                success: function () {
                    toastr['success']('', 'Notifikacija je označena kao bitna');
                    element.important = true;
                },
                error: function () {
                    HandleError('Došlo je do greške', 'Molimo pokušajte ponovo ili kontaktirajte korisničku podršku.', 'error');
                }
            });
        }
    })

    let markRead = notificationsHolder.find('#mark-read-main');
    markRead.attr('id', 'mark-read-' + element.id);

    if (element.read === true) {
        checkIcon.addClass('text-primary far fa-check-double');
        read.text(' Pročitano');

        markRead.remove();
    } else {
        notificationListMain.addClass('notification-list--unread');
        checkIcon.addClass('far fa-check');
        read.text(' Nepročitano');

        let smallMarkRead = notificationsHolder.find('#small-mark-read-main');
        smallMarkRead.attr('id', 'small-mark-read-' + element.id);

        markRead.click(function () {
            smallMarkRead.addClass('no-tick');
            const width = paragraphMarkRead.width();
            const height = paragraphMarkRead.height();
            smallMarkRead.fadeOut('fast', function () {
                $(this).html('<i class="fas fa-spinner fa-spin text-primary no-tick"></i>').fadeIn('fast');
                paragraphMarkRead.width(width);
                paragraphMarkRead.height(height);
            })

            $.ajax({
                url: url + "SetRead",
                method: "POST",
                data: { notificationId: element.id },
                success: setTimeout(function () {
                    //smallMarkRead.remove();
                    //checkIcon.removeClass('fa-check');
                    //checkIcon.addClass('fa-check-double');
                    //checkIcon.addClass('text-primary');
                    //read.text(' Read');

                    //notificationsHolder.fadeOut('slow');

                    $(this).promise().done(function () {
                        smallMarkRead.fadeOut('fast', function () {
                            markRead.remove();
                        }).promise().done(function () {
                            importantStar.click(function () {
                                importantStar.toggleClass('fas');
                                importantStar.toggleClass('fal');
                                importantStar.toggleClass('important-star--true')

                                if (element.important === true) {
                                    $.ajax({
                                        url: url + "SetUnimportant",
                                        method: "POST",
                                        data: { notificationId: element.id },
                                        success: function () {
                                            toastr['success']('', 'Notifikacija je uklonjena iz bitnih');
                                            element.important = false;
                                        },
                                        error: function () {
                                            HandleError('Došlo je do greške', 'Molimo pokušajte ponovo ili kontaktirajte korisničku podršku.', 'error');
                                        }
                                    });
                                } else {
                                    $.ajax({
                                        url: url + "SetImportant",
                                        method: "POST",
                                        data: { notificationId: element.id },
                                        success: function () {
                                            toastr['success']('', 'Notifikacija je označena kao bitna');
                                            element.important = true;
                                        },
                                        error: function () {
                                            HandleError('Došlo je do greške', 'Molimo pokušajte ponovo ili kontaktirajte korisničku podršku.', 'error');
                                        }
                                    });
                                }
                            })

                            $(this).html(importantStar);
                            $(this).fadeIn('slow');
                        });

                        checkIcon.fadeOut('fast', function () {
                            $(this).removeClass('fa-check');
                            $(this).addClass('fa-check-double');
                            $(this).addClass('text-primary');
                            $(this).fadeIn('slow');
                        })

                        read.fadeOut('fast', function () {
                            $(this).text(' Pročitano').fadeIn('slow');
                        });

                        notificationListMain.removeClass('notification-list--unread').addClass('notification-list--read');
                    })

                    var counter = $('#unread-notifications');

                    unreadCount--;

                    if (unreadCount === 0) {
                        //counter.removeClass('notify');
                        //counter.removeClass('show-count');
                        $(this).promise().done(function () {
                            $('#unread-notifications').fadeOut('fast');
                        })
                    } else {
                        counter.html(unreadCount);
                    }

                    element.read = true;

                    toastr['success']('', 'Notifikacija je označena kao pročitana');
                }, 1000),
                error: function () {
                    HandleError('Došlo je do greške', 'Molimo pokušajte ponovo ili kontaktirajte korisničku podršku.', 'error');
                }
            })
        })
    }

    let notificationIconMain = notificationsHolder.find('#notification-icon-main');
    notificationIconMain.attr('id', 'notification-icon-' + element.id);
    notificationIconMain.addClass('text-' + element.severity);
    notificationIconMain.addClass(element.icon);

    let notificationText = notificationsHolder.find('#notification-text-main');
    notificationText.attr('id', 'notification-text-' + element.id);
    notificationText.text(element.title);

    let daysAgo = notificationsHolder.find('#days-ago-main');
    daysAgo.attr('id', 'days-ago-' + element.id);
    daysAgo.text(element.sendingDateTime);
    notificationsHolder.show();
    notificationsHolder.appendTo('#notification-center-content');
}

function SetUnreadNotificationCounter(count) {
    var el = $('#unread-notifications');
    el.hide();

    if (unreadCount > 0) {
        el.fadeIn('fast');
        el.html(unreadCount);
    }
}

function HandleError(title, body, severity) {
    toastr[severity](body, title);
}

GetAllNotifications();

let connection = new signalR.HubConnectionBuilder().withUrl(notificationHubUrl).build();

connection.on('displayNotification', () => {
    GetAllNotifications();
});

connection.start();