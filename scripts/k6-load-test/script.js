

import http from 'k6/http';
import { Counter } from 'k6/metrics';

// Sayaçlar
export const successCount = new Counter('successful_requests');
export const failCount = new Counter('sfailed_requests');

export const options = {
  vus: 50, // Eşzamanlı kullanıcı sayısı
  duration: '5s', // Test süresi
};

export default function () {
  // HTTP GET isteği
  const res = http.get('https://localhost:7234/api/resilience/rate-limiter');


  try {
    // Yanıt gövdesini JSON olarak ayrıştır
    const body = JSON.parse(res.body);

    // Gövde içindeki statusCode kontrolü
    if (body.statusCode === 200) {
      successCount.add(1); // Başarılı istek
    } else {
      failCount.add(1); // Başarısız istek
    }
  } catch (e) {
    console.error(`Yanıt JSON olarak ayrıştırılamadı: ${e.message}`);
    failCount.add(1); // JSON ayrıştırma hatalarını başarısız olarak say
  }
}
