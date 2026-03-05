self.addEventListener('push', (event) => {
  const data = event.data ? event.data.json() : { title: 'Machinery Dashboard', body: 'Status update' };
  event.waitUntil(
    self.registration.showNotification(data.title, {
      body: data.body,
      icon: '/icon.png',
      badge: '/icon.png'
    })
  );
});

self.addEventListener('notificationclick', (event) => {
  event.notification.close();
  event.waitUntil(clients.openWindow('/'));
});
